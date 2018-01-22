using Global.Domain;
using NSLDS.Domain;
using Microsoft.Extensions.Configuration;
using Quartz;
using System;
using System.Linq;
using System.IO;
using System.Text;
using Serilog;
using System.Net.Mail;
using NSLDS.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace NSLDS.API
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
        private DateTime JobDate { get; set; }
        private DateTime SubmitDate { get; set; }
        private int Sequence { get; set; }
        private string User { get; set; }
        private int BatchId { get; set; }

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
                    msg.To.Add(new MailAddress(Configuration["MailNotifications:MailTo"]));
                    msg.From = new MailAddress(Configuration["MailNotifications:MailFrom"]);
                    msg.Subject = Configuration["MailNotifications:Subject"];
                    var mailbody = new StringBuilder()
                        .AppendLine()
                        .AppendLine("DataCheck Manual Response Job Notifications");
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
                    smtp.Host = Configuration["MailNotifications:Server"];
                    smtp.Port = Configuration.GetValue<int>("MailNotifications:Port");
                    smtp.Send(msg);
                    return true;
                }
            }
            catch { return false; }
        }

        private bool ProcessResponses()
        {
            var RootPath = Configuration["AppSettings:UploadRootFolder"];
            var JobPath = Path.Combine(RootPath, TenantId, JobDate.ToString("yyyy-MM-dd"), "queue");
            var clientPath = Path.Combine(JobPath, "pending");
            var errorPath = Path.Combine(JobPath, "error");
            var archivePath = Path.Combine(JobPath, "processed");

            if (!Directory.Exists(clientPath))
            {
                Directory.CreateDirectory(clientPath);
            }

            if (!Directory.Exists(errorPath))
            {
                Directory.CreateDirectory(errorPath);
            }

            if (!Directory.Exists(archivePath))
            {
                Directory.CreateDirectory(archivePath);
            }

            var trnFiles = Directory.GetFiles(clientPath, $"{FileNameConstant.TRNINFOP}.{BatchId}");
            var fahFiles = Directory.GetFiles(clientPath, $"{FileNameConstant.FAHEXTOP}.{BatchId}");
            var traFiles = Directory.GetFiles(clientPath, $"{FileNameConstant.TRALRTOP}.{BatchId}");
            
            // check that there is something to process else return true (no rescheduling)
            if (trnFiles.Count() == 0 && fahFiles.Count() == 0 && traFiles.Count() == 0)
            {
                return true;
            }

            // instantiate the response processor
            var resp = new ResponseProcessor();
            resp.GlobalContext = GlobalContext;
            resp.NsldsContext = NsldsContext;
            resp.ErrorPath = errorPath;

            // process TRNINFOP files first as FAHEXTOP only processes after
            foreach (var trnFile in trnFiles)
                // datachk-166: transaction implemented for each individual response file
                using (var dbtrans = NsldsContext.Database.BeginTransaction())
                {
                    try
                    {
                        bool success = resp.Run(trnFile, TenantId, BatchId);
                        // datachk-166: commit dbtrans if no exception
                        dbtrans.Commit();

                        // check for error batches saved
                        if (resp.HasErrors)
                        {
                            LogWarning($"TRNINFOP batch errors were found and saved in {errorPath}");
                        }
                        // archive and move file to processed
                        var newFile = $"{DateTime.Now.ToString("HH'h'mm_")}{Path.GetFileName(trnFile)}";
                        var destFile = Path.Combine(archivePath, newFile);
                        //if (File.Exists(destFile)) { File.Delete(destFile); }
                        File.Move(trnFile, destFile);

                        if (success)
                        {
                            Logger.Information($"Processed: {trnFile}");
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
                        return false;
                    }
                }

            // process FAHEXTOP files
            foreach (var fahFile in fahFiles)
                // datachk-166: transaction implemented for each individual response file
                using (var dbtrans = NsldsContext.Database.BeginTransaction())
                {
                    try
                    {
                        bool success = resp.Run(fahFile, TenantId, BatchId);
                        // datachk-166: commit dbtrans if no exception
                        dbtrans.Commit();

                        // check for error batches saved
                        if (resp.HasErrors)
                        {
                            LogWarning($"FAHEXTOP batch errors were found and saved in {errorPath}");
                        }
                        // archive and move file to processed
                        var newFile = $"{DateTime.Now.ToString("HH'h'mm_")}{Path.GetFileName(fahFile)}";
                        var destFile = Path.Combine(archivePath, newFile);
                        //if (File.Exists(destFile)) { File.Delete(destFile); }
                        File.Move(fahFile, destFile);

                        if (success)
                        {
                            Logger.Information($"Processed: {fahFile}");
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
                        return false;
                    }
                }

            // process TRALRTOP files
            foreach (var traFile in traFiles)
                // datachk-166: transaction implemented for each individual response file
                using (var dbtrans = NsldsContext.Database.BeginTransaction())
                {
                    try
                    {
                        bool success = resp.Run(traFile, TenantId, BatchId);
                        // datachk-166: commit dbtrans if no exception
                        dbtrans.Commit();

                        // check for error batches saved
                        if (resp.HasErrors)
                        {
                            LogWarning($"TRALRTOP batch errors were found and saved in {errorPath}");
                        }
                        // archive and move file to processed
                        var newFile = $"{DateTime.Now.ToString("HH'h'mm_")}{Path.GetFileName(traFile)}";
                        var destFile = Path.Combine(archivePath, newFile);
                        //if (File.Exists(destFile)) { File.Delete(destFile); }
                        File.Move(traFile, destFile);

                        if (success)
                        {
                            Logger.Information($"Processed: {traFile}");
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
                        return false;
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
                JobDate = (DateTime)dm["jobdate"];
                User = dm["user"].ToString();
                var batch = dm["batch"] as ResponseBatchResult;
                BatchId = batch.BatchId;
                SubmitDate = batch.SubmittedOn;
                Sequence = batch.Sequence;
                var maxjobs = Configuration.GetValue<int>("AppSettings:QueueMaxJobs");
                var jobdelay = Configuration.GetValue<int>("AppSettings:QueueJobDelay");

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
                    messageOut = $"{TenantId}: Response Job for batch {BatchId} is delayed.";
                    return;
                }

                Console.WriteLine($"{TenantId}: Response Job for batch {BatchId} is starting at {startTime}.");

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
                var success = ProcessResponses();
                if (!success)
                {
                    messageOut = $"DataCheck manual response job for batch {BatchId} did not complete and has been rescheduled.";
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
                    messageOut = $"{TenantId}: Response Job for batch {BatchId} is ending at {DateTime.Now} (total time {DateTime.Now.Subtract(startTime).ToString("hh'h'mm'm'ss's'")}).";
                }
            }
            catch (Exception ex)
            {
                // only exception type that doesn't shut down the process
                // can be used in conjunction with scheduler listeners
                //throw new JobExecutionException(ex);
                LogError($"DataCheck manual response job failed at: {DateTime.Now} -> {ex.Message} {ex.InnerException?.Message}");
                messageOut = $"{TenantId}: Response Job for batch {BatchId} is ending at {DateTime.Now} (total time {DateTime.Now.Subtract(startTime).ToString("hh'h'mm'm'ss's'")}).";
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
