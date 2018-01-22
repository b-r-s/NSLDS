using Global.Domain;
using NSLDS.Domain;
using NSLDS.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Quartz;
using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Text;
using Serilog;
using System.Net.Mail;
using System.Threading;

namespace NSLDS.Scheduler
{
    [DisallowConcurrentExecution]
    public class ResponseJobProcessor : IJob
    {
        #region Private Fields and Methods

        private GlobalContext GlobalContext { get; set; }
        private NSLDS_Context NsldsContext { get; set; }
        private IHostingEnvironment HostingEnvironment { get; set; }
        private IConfiguration Configuration { get; set; }
        private ILogger Logger { get; set; }
        private StringBuilder Warnings { get; set; } = new StringBuilder();
        private StringBuilder Errors { get; set; } = new StringBuilder();
        private string TenantId { get; set; }

        private void LogWarning(string message)
        {
            Warnings.AppendLine($"[{TenantId}]: {message}");
            Logger.Warning($"[{TenantId}]: {message}");
        }

        private void LogError(string message)
        {
            Errors.AppendLine($"[{TenantId}]: {message}");
            Logger.Error($"[{TenantId}]: {message}");
        }

        private bool EmailNotification()
        {
            if (Warnings.Length == 0 && Errors.Length == 0)
            {
                // nothing to email
                return true;
            }

            try
            {
                using (var msg = new MailMessage())
                using (var smtp = new SmtpClient())
                {
                    msg.IsBodyHtml = false;
                    msg.To.Add(new MailAddress(Configuration["MailSettings:MailTo"]));
                    msg.From = new MailAddress(Configuration["MailSettings:MailFrom"]);
                    msg.Subject = Configuration["MailSettings:Subject"];
                    var mailbody = new StringBuilder()
                        .AppendLine()
                        .AppendLine("DataCheck Scheduler Receive Job Notifications");
                    if (Warnings.Length > 0)
                    {
                        mailbody.AppendLine().AppendLine("Warnings:");
                        mailbody.Append(Warnings);
                    }
                    if (Errors.Length > 0)
                    {
                        mailbody.AppendLine().AppendLine("Errors:");
                        mailbody.Append(Errors);
                    }
                    msg.Body = mailbody.ToString();
                    smtp.Host = Configuration["MailSettings:Server"];
                    smtp.Port = Configuration.GetValue<int>("MailSettings:Port");
                    smtp.Send(msg);
                    return true;
                }
            }
            catch { return false; }
        }

        private bool ProcessResponses(ClientProfile cp, NSLDS_Context context)
        {
            var RootPath = Configuration["TDClientSettings:RootFolder"];
            var clientPath = Path.Combine(RootPath, cp.OPEID, "queue");
            var pendingPath = Path.Combine(RootPath, cp.OPEID, "pending");
            var errorPath = Path.Combine(pendingPath, "error");
            var archivePath = Path.Combine(RootPath, cp.OPEID, "processed");

            if (!Directory.Exists(clientPath))
            {
                Directory.CreateDirectory(clientPath);
                return true; // obviously nothing to process
            }

            if (!Directory.Exists(errorPath))
            {
                Directory.CreateDirectory(errorPath);
            }

            if (!Directory.Exists(archivePath))
            {
                Directory.CreateDirectory(archivePath);
            }

            // check that all the queued files have valid names
            var allFiles = Directory.GetFiles(clientPath);
            
            var trnFiles = Directory.GetFiles(clientPath, $"{FileNameConstant.TRNINFOP}.*");
            var fahFiles = Directory.GetFiles(clientPath, $"{FileNameConstant.FAHEXTOP}.*");
            var traFiles = Directory.GetFiles(clientPath, $"{FileNameConstant.TRALRTOP}.*");

            if (allFiles.Count() != trnFiles.Count() + fahFiles.Count() + traFiles.Count())
            {
                // move invalid filenames to error folder and log warning
                foreach (var file in allFiles)
                {
                    if (trnFiles.Contains(file) || fahFiles.Contains(file) || traFiles.Contains(file))
                    {
                        continue;
                    }
                    else
                    {
                        File.Move(file, Path.Combine(errorPath, Path.GetFileName(file)));
                        LogWarning($"Invalid filename {Path.GetFileName(file)} moved to {errorPath}");
                    }
                }
            }

            // check that there is something to process
            if (trnFiles.Count() == 0 && fahFiles.Count() == 0 && traFiles.Count() == 0)
            {
                return true;
            }

            // instantiate the response processor
            var resp = new ResponseProcessor();
            resp.GlobalContext = GlobalContext;
            resp.NsldsContext = context;
            resp.ErrorPath = errorPath;

            // process TRNINFOP files first as FAHEXTOP only processes after
            foreach (var trnFile in trnFiles)
                // datachk-166: transaction implemented for each individual response file
                using (var dbtrans = NsldsContext.Database.BeginTransaction())
                {
                    try
                    {
                        var start = DateTime.Now;
                        bool success = resp.Run(trnFile, cp.OPEID);
                        // datachk-166: commit dbtrans if no exception
                        dbtrans.Commit();

                        // check for error batches saved
                        if (resp.HasErrors)
                        {
                            LogWarning($"TRNINFOP batch errors were found and saved in {errorPath}");
                        }
                        // archive and move file to processed
                        var newFile = $"{DateTime.Now.ToString("yyyy-MM-dd_HH'h'mm_")}{Path.GetFileName(trnFile)}";
                        var destFile = Path.Combine(archivePath, newFile);
                        //if (File.Exists(destFile)) { File.Delete(destFile); }
                        File.Move(trnFile, destFile);

                        if (success)
                        {
                            Logger.Information($"Processed: {trnFile} ({DateTime.Now.Subtract(start).ToString("hh'h'mm'm'ss's'")})");
                        }
                        else
                        {
                            LogWarning($"Incomplete: {trnFile}");
                            LogError(string.Join(", ", resp.ErrorMessage));
                        }
                    }
                    catch (Exception ex)
                    {
                        dbtrans.Rollback();
                        LogWarning($"Warning: response {trnFile} failed and is being rescheduled -> {ex.Message} {ex.InnerException?.Message}");
                    }
                }

            // process FAHEXTOP files
            foreach (var fahFile in fahFiles)
                // datachk-166: transaction implemented for each individual response file
                using (var dbtrans = NsldsContext.Database.BeginTransaction())
                {
                    try
                    {
                        var start = DateTime.Now;
                        bool success = resp.Run(fahFile, cp.OPEID);
                        // datachk-166: commit dbtrans if no exception
                        dbtrans.Commit();

                        // check for error batches saved
                        if (resp.HasErrors)
                        {
                            LogWarning($"FAHEXTOP batch errors were found and saved in {errorPath}");
                        }
                        // archive and move file to processed unless marked skip = true
                        if (!resp.Skip)
                        {
                            var newFile = $"{DateTime.Now.ToString("yyyy-MM-dd_HH'h'mm_")}{Path.GetFileName(fahFile)}";
                            var destFile = Path.Combine(archivePath, newFile);
                            //if (File.Exists(destFile)) { File.Delete(destFile); }
                            File.Move(fahFile, destFile);
                        }

                        if (success)
                        {
                            Logger.Information($"Processed: {fahFile} ({DateTime.Now.Subtract(start).ToString("hh'h'mm'm'ss's'")})");
                        }
                        else
                        {
                            LogWarning($"Incomplete: {fahFile}");
                            LogError(string.Join(", ", resp.ErrorMessage));
                        }
                    }
                    catch (Exception ex)
                    {
                        dbtrans.Rollback();
                        LogWarning($"Warning: response {fahFile} failed and is being rescheduled -> {ex.Message} {ex.InnerException?.Message}");
                    }
                }

            // process TRALRTOP files
            foreach (var traFile in traFiles)
                // datachk-166: transaction implemented for each individual response file
                using (var dbtrans = NsldsContext.Database.BeginTransaction())
                {
                    try
                    {
                        var start = DateTime.Now;
                        bool success = resp.Run(traFile, cp.OPEID);
                        // datachk-166: commit dbtrans if no exception
                        dbtrans.Commit();

                        // check for error batches saved
                        if (resp.HasErrors)
                        {
                            LogWarning($"TRALRTOP batch errors were found and saved in {errorPath}");
                        }
                        // archive and move file to processed
                        var newFile = $"{DateTime.Now.ToString("yyyy-MM-dd_HH'h'mm_")}{Path.GetFileName(traFile)}";
                        var destFile = Path.Combine(archivePath, newFile);
                        //if (File.Exists(destFile)) { File.Delete(destFile); }
                        File.Move(traFile, destFile);

                        if (success)
                        {
                            Logger.Information($"Processed: {traFile} ({DateTime.Now.Subtract(start).ToString("hh'h'mm'm'ss's'")})");
                        }
                        else
                        {
                            LogWarning($"Incomplete: {traFile}");
                            LogError(string.Join(", ", resp.ErrorMessage));
                        }
                    }
                    catch (Exception ex)
                    {
                        dbtrans.Rollback();
                        LogWarning($"Warning: response {traFile} failed and is being rescheduled -> {ex.Message} {ex.InnerException?.Message}");
                    }
                }

            return true;
        }

        #endregion

        #region Public Methods

        public void Execute(IJobExecutionContext context)
        {
            var startTime = DateTime.Now;
            var messageOut = string.Empty;
            try
            {
                // retrieve context object instances
                var _context = context.Scheduler.Context;
                HostingEnvironment = (IHostingEnvironment)_context.Get("_env");
                Configuration = (IConfiguration)_context.Get("_config");
                Logger = (ILogger)_context.Get("_logger");

                var dm = context.JobDetail.JobDataMap;
                TenantId = dm["opeid"].ToString();
                var maxjobs = Configuration.GetValue<int>("TDClientSettings:QueueMaxJobs");
                var jobdelay = Configuration.GetValue<int>("TDClientSettings:QueueJobDelay");

                // since each job is unique and we don't want all jobs to run
                // at the same time, we have to reschedule until the queue is empty
                // datachk-166: only allow 1 response processing job per OPEID at a time
                // to allow other concurrent OPEID processing jobs up to the defined QueueMaxJobs

                var alljobs = context.Scheduler.GetCurrentlyExecutingJobs();
                var processjobs = alljobs.Where(x => x.JobDetail.JobType == typeof(ResponseJobProcessor));
                var opeidjobs = processjobs.Count(x => x.JobDetail.Key.Group == TenantId);
                if (processjobs.Count() > maxjobs || opeidjobs > 1)
                {
                    var oldTrigger = context.Trigger.Key;
                    var newTrigger = TriggerBuilder.Create()
                        .ForJob(context.JobDetail)
                        .StartAt(DateTime.UtcNow.AddSeconds(jobdelay))
                        .Build();
                    context.Scheduler.RescheduleJob(oldTrigger, newTrigger);
                    messageOut = $"{TenantId}: Response Job for batch {context.JobDetail.Key} is delayed.";
                    return;
                }

                Console.WriteLine($"{TenantId}: Response Job for batch {context.JobDetail.Key} is starting at {startTime}.");
                // retrieve GlobalDb connection string
                var connectionString = Configuration["Data:GlobalDb:ConnectionString"];
                var optionsbuilder = new DbContextOptionsBuilder();
                optionsbuilder.UseSqlServer(connectionString);
                GlobalContext = new GlobalContext(optionsbuilder.Options);
                // configure client db connection
                var dbname = GlobalContext.Tenants.Single(x => x.TenantId == TenantId).DatabaseName;
                var conn = string.Format(Configuration["Data:ClientDb:ConnectionString"], dbname);
                var cob = new DbContextOptionsBuilder();
                cob.UseSqlServer(conn);
                NsldsContext = new NSLDS_Context(cob.Options);
                var cp = NsldsContext.ClientProfiles.Single(x => x.OPEID == TenantId);
                var success = ProcessResponses(cp, NsldsContext);
                if (!success)
                {
                    messageOut = $"DataCheck Scheduler response job for batch {context.JobDetail.Key} did not complete and has been rescheduled.";
                    LogWarning(messageOut);
                    var oldTrigger = context.Trigger.Key;
                    var newTrigger = TriggerBuilder.Create()
                        .ForJob(context.JobDetail)
                        .StartAt(DateTime.UtcNow.AddSeconds(jobdelay))
                        .Build();
                    context.Scheduler.RescheduleJob(oldTrigger, newTrigger);
                }
                else
                {
                    messageOut = $"{TenantId}: Response Job for batch {context.JobDetail.Key} is ending at {DateTime.Now} (total time {DateTime.Now.Subtract(startTime).ToString("hh'h'mm'm'ss's'")}).";
                }
            }
            catch (Exception ex)
            {
                // only exception type that doesn't shut down the process
                // can be used in conjunction with scheduler listeners
                //throw new JobExecutionException(ex);
                LogError($"DataCheck Scheduler response job failed at: {DateTime.Now} -> {ex.Message} {ex.InnerException?.Message}");
                messageOut = $"{TenantId}: Response Job for batch {context.JobDetail.Key} is ending at {DateTime.Now} (total time {DateTime.Now.Subtract(startTime).ToString("hh'h'mm'm'ss's'")}).";
            }
            finally
            {
                EmailNotification();
                if (GlobalContext != null) { GlobalContext.Dispose(); }
                if (NsldsContext != null) { NsldsContext.Dispose(); }
                Console.WriteLine(messageOut);
            }
        }

        #endregion
    }
}
