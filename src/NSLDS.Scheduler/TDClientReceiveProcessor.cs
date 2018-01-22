using Global.Domain;
using NSLDS.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Text;
using Serilog;
using System.Net.Mail;
using NSLDS.Common;
using System.Threading;

namespace NSLDS.Scheduler
{
    [DisallowConcurrentExecution]
    public class TDClientReceiveJob : IJob
    {
        #region Private Fields and Methods

        private GlobalContext GlobalContext { get; set; }
        private IHostingEnvironment HostingEnvironment { get; set; }
        private IConfiguration Configuration { get; set; }
        private ILogger Logger { get; set; }
        private StringBuilder Warnings { get; set; } = new StringBuilder();
        private StringBuilder Errors { get; set; } = new StringBuilder();
        private string TenantId = string.Empty;

        private string DecodePassword(string encodedData)
        {
            try
            {
                System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
                System.Text.Decoder utf8Decode = encoder.GetDecoder();
                byte[] todecode_byte = Convert.FromBase64String(encodedData);
                int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
                char[] decoded_char = new char[charCount];
                utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
                string result = new String(decoded_char);
                return result;
            }
            catch
            {
                return null;
            }
        }

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

        private bool ExecuteBatch(string command, ClientProfile cp, NSLDS_Context context)
        {
            // build batch command configuration
            string batchName = Configuration[$"TDClientSettings:{command}"];
            string batchPath = Configuration["TDClientSettings:RootFolder"];

            // datachk-209: check if pending folder exists before calling tdclient receive
            var sourcePath = Path.Combine(batchPath, cp.OPEID, "pending");
            if (!Directory.Exists(sourcePath))
            { Directory.CreateDirectory(sourcePath); }

            var destinationPath = Path.Combine(batchPath, cp.OPEID, "queue");
            if (!Directory.Exists(destinationPath))
            { Directory.CreateDirectory(destinationPath); }

            string batchParams = $"{cp.SAIG} {DecodePassword(cp.TD_Password)} {cp.OPEID} {batchPath}";
            string batchRun = $"{batchPath}{batchName}";
            if (!File.Exists(batchRun))
            {
                LogError($"Batch file not found {batchRun}");
                return false;
            }

            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("cmd.exe", $"/c {batchRun} {batchParams}");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            process = Process.Start(processInfo);

            // *** Read the streams ***
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit(300000); // wait 5 minutes max
            exitCode = process.ExitCode;

            //Logger.Information("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
            //Logger.Information("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
            //Logger.Information("ExitCode: " + exitCode.ToString(), "ExecuteCommand");
            process.Close();

            // check login
            if (output.Contains("FTP login failed"))
            {
                cp.IsPwdValid = false;
                context.Entry(cp).State = EntityState.Modified;
                context.SaveChanges();
                return false;
            }
            if (output.Contains("SUCCESS"))
            {
                // datachk-166
                // move received response files from pending to queue folder using unique extensions
                if (command == "receive")
                {
                    var files = Directory.GetFiles(sourcePath);
                    if (files.Count() == 0) { return false; }

                    Array.ForEach(files, file => 
                    {
                        var newfile = Path.ChangeExtension(file, DateTime.Now.Ticks.ToString());
                        File.Move(file, Path.Combine(destinationPath, Path.GetFileName(newfile)));
                        Thread.Sleep(1);
                    });
                    // datachk-166: check the queue folder for responses awaiting
                    if (Directory.GetFiles(destinationPath).Count() == 0) { return false; }
                }
                return true;
            }
            // unknown SAIG portal error, log it
            LogError(output);
            if (!string.IsNullOrEmpty(error)) { Logger.Error(error); }
            return false;
        }

        private void CheckAndMoveTdConnectFiles(ClientProfile cp)
        {
            // workaround for APU files picked up by Global EdConnect mailbox only
            if (cp.SAIG != "TG51025" || cp.OPEID != "03819300") { return; }

            var AltPath = Configuration["TDClientSettings:AltFolder"];
            var RootPath = Configuration["TDClientSettings:RootFolder"];
            var clientPath = Path.Combine(RootPath, cp.OPEID, "pending");

            if (!Directory.Exists(clientPath))
            {
                Directory.CreateDirectory(clientPath);
            }

            try
            {
                //Check TDConnect folder
                var trnFiles = Directory.GetFiles(AltPath, $"{FileNameConstant.TRNINFOP}.*");
                var fahFiles = Directory.GetFiles(AltPath, $"{FileNameConstant.FAHEXTOP}.*");
                var traFiles = Directory.GetFiles(AltPath, $"{FileNameConstant.TRALRTOP}.*");

                if (trnFiles.Count() == 0 && fahFiles.Count() == 0 && traFiles.Count() == 0) { return; }

                foreach (var trnFile in trnFiles)
                {
                    var destFile = Path.Combine(clientPath, Path.GetFileName(trnFile));
                    if (!File.Exists(destFile)) { File.Move(trnFile, destFile); }
                }

                foreach (var fahFile in fahFiles)
                {
                    var destFile = Path.Combine(clientPath, Path.GetFileName(fahFile));
                    if (!File.Exists(destFile)) { File.Move(fahFile, destFile); }
                }

                foreach (var traFile in traFiles)
                {
                    var destFile = Path.Combine(clientPath, Path.GetFileName(traFile));
                    if (!File.Exists(destFile)) { File.Move(traFile, destFile); }
                }

                Logger.Information($"{trnFiles.Count() + fahFiles.Count() + traFiles.Count()} response files moved from {AltPath}");
            }
            catch (Exception ex)
            {
                LogError($"Error: {ex.Message} {ex.InnerException?.Message}");
            }
        }

        #endregion

        #region Public Methods

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                var startTime = DateTime.Now;
                //Console.Clear(); // clear console buffer

                // retrieve context object instances
                var _context = context.Scheduler.Context;
                HostingEnvironment = (IHostingEnvironment)_context.Get("_env");
                Configuration = (IConfiguration)_context.Get("_config");
                Logger = (ILogger)_context.Get("_logger");
                Logger.Information($"NSLDS.Scheduler receive job starting at: {startTime}");

                // retrieve GlobalDb connection string
                var connectionString = Configuration["Data:GlobalDb:ConnectionString"];
                var optionsbuilder = new DbContextOptionsBuilder();
                optionsbuilder.UseSqlServer(connectionString);
                GlobalContext = new GlobalContext(optionsbuilder.Options);
                // retrieve active client databases
                var tenants = GlobalContext.Tenants
                    .Where(x => x.IsActive).ToList();

                foreach (var item in tenants)
                    try
                    {
                        TenantId = item.TenantId;
                        Logger.Information($"{TenantId} is active on {item.DatabaseName}");
                        var conn = string.Format(Configuration["Data:ClientDb:ConnectionString"], item.DatabaseName);
                        var cob = new DbContextOptionsBuilder();
                        cob.UseSqlServer(conn);
                        using (var NsldsContext = new NSLDS_Context(cob.Options))
                        {
                            try
                            {
                                NsldsContext.Database.OpenConnection();
                            }
                            catch
                            {
                                LogWarning($"Invalid client database {item.DatabaseName}");
                                continue;
                            }

                            var cp = NsldsContext.ClientProfiles.SingleOrDefault(x => x.OPEID == item.TenantId);
                            if (cp == null)
                            {
                                LogWarning($"Client profile not found for {item.TenantId}");
                                continue;
                            }
                            // nslds-110 check upload_method in client profile
                            string[] manualUpload = { UploadMethod.TdManual, UploadMethod.EdConnect };
                            if (manualUpload.Contains(cp.Upload_Method))
                            {
                                Logger.Information("Client upload mechanism set to manual, skipped");
                                continue;
                            }
                            else if (cp.Upload_Method == UploadMethod.Empty)
                            {
                                LogWarning("Upload mechanism has not been defined in the client profile");
                                continue;
                            }
                            if (!cp.IsPwdValid)
                            {
                                LogWarning("TD Client password was marked invalid due to a previous failed login attempt");
                                continue;
                            }
                            Logger.Information("Checking TD Client login");
                            var success = ExecuteBatch("login", cp, NsldsContext);
                            if (!success)
                            {
                                if (cp.IsPwdValid)
                                    LogWarning("TD Client login failed, SAIG portal unavailable");
                                else
                                    LogWarning("TD Client login failed, check SAIG mailbox and password settings");
                                continue;
                            }

                            // passed the login check, proceed with batch requests receive
                            //  workaround for files picked up by tdconnect before this job
                            // move files to pending first
                            CheckAndMoveTdConnectFiles(cp);

                            // run receive from nslds batch command
                            Logger.Information("Checking SAIG mailbox for response files");
                            success = ExecuteBatch("receive", cp, NsldsContext);
                            if (success) // response files have been received
                            {
                                // start a processing job for queued response files
                                Logger.Information("Queueing response files, please wait...");
                                //success = ProcessResponses(cp, NsldsContext);
                                // configure our scheduler service access
                                var jobdata = new JobDataMap();
                                jobdata.Add("opeid", cp.OPEID);
                                jobdata.Add("jobdate", DateTime.Today);
                                // define the jobs and tie it to our ResponseJobProcessor classes
                                var job = JobBuilder.Create<ResponseJobProcessor>()
                                    .WithIdentity(DateTime.Now.Ticks.ToString(), cp.OPEID)
                                    .UsingJobData(jobdata)
                                    .Build();

                                var jobdelay = Configuration.GetValue<int>("TDClientSettings:QueueJobDelay");

                                var trigger = TriggerBuilder.Create()
                                    .StartAt(DateTime.UtcNow.AddSeconds(jobdelay)) // delay defined in appsettings
                                    .Build();
                                // place job in queue
                                context.Scheduler.ScheduleJob(job, trigger);
                            }
                            Logger.Information((success) ? $"Responses have been queued for {cp.Organization_Name}" : $"No responses to process for {cp.Organization_Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"{item.DatabaseName} receive job failed: {ex.Message} {ex.InnerException?.Message}");
                    }
                Logger.Information($"NSLDS.Scheduler receive job finished at: {DateTime.Now}");
            }
            catch (Exception ex)
            {
                // only exception type that doesn't shut down the process
                // can be used in conjunction with scheduler listeners
                //throw new JobExecutionException(ex);
                LogError($"NSLDS.Scheduler receive job failed: {ex.Message} {ex.InnerException?.Message}");
            }
            finally
            {
                EmailNotification();
                if (GlobalContext != null) { GlobalContext.Dispose(); }
                Logger.Information($"NSLDS.Scheduler next receive job will start at: {context.Trigger.GetFireTimeAfter(DateTime.Now)}");
            }
        }
    }

    #endregion
}
