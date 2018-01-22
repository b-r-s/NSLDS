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

namespace NSLDS.Scheduler
{
    [DisallowConcurrentExecution]
    public class TDClientSendJob : IJob
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
                        .AppendLine("DataCheck Scheduler Send Job Notifications");
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

        // datachk-68: helper function for TM alert refresh expiration check
        private DateTime ValidUntil(DateTime date, int days)
        {
            var expdate = date.AddDays(0);

            while (days > 0)
            {
                expdate = expdate.AddDays(1);
                if (expdate.DayOfWeek < DayOfWeek.Saturday &&
                    expdate.DayOfWeek > DayOfWeek.Sunday)
                    days--;
            }
            return expdate;
        }

        private bool ExecuteBatch(string command, ClientProfile cp, NSLDS_Context context)
        {
            // build batch command configuration
            string batchName = Configuration[$"TDClientSettings:{command}"];
            string batchPath = Configuration["TDClientSettings:RootFolder"];
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
                // if send command, move files to sent folder
                if (command == "send")
                {
                    var sourcePath = Path.Combine(batchPath, cp.OPEID);
                    var destinationPath = Path.Combine(sourcePath, "sent");
                    if (!Directory.Exists(destinationPath))
                    { Directory.CreateDirectory(destinationPath); }

                    var files = Directory.GetFiles(sourcePath, "*.txt");
                    Array.ForEach(files, file => File.Move(file, Path.Combine(destinationPath, Path.GetFileName(file))));
                }
                return true;
            }
            // unknown SAIG portal error, log it
            LogError(output);
            if (!string.IsNullOrEmpty(error)) { Logger.Error(error); }
            return false;
        }

        private bool GenerateOutputFiles(ClientProfile cp, NSLDS_Context context)
        {
            var RootPath = Configuration["TDClientSettings:RootFolder"];
            var clientPath = Path.Combine(RootPath, cp.OPEID);
            if (!Directory.Exists(clientPath))
            {
                Directory.CreateDirectory(clientPath);
            }

            // retrieve batch requests ready to be submitted
            var cBatches = context.ClientRequests
                .Where(x => x.Link2ClientProfile == cp.Id && !x.IsSubmitted &&
                    x.SubmittedOn < DateTime.Now && !x.IsDeleted && !x.IsOnHold)
                .ToList();

            // retrieve the last sequence# for today's submitted batch requests
            short seq = context.ClientRequests
                .Where(x => x.Link2ClientProfile == cp.Id && x.SubmittedOn.HasValue &&
                    x.SubmittedOn.Value.Date == DateTime.Today && x.IsSubmitted)
                .Max(x => x.Sequence as short?) ?? 0;

            // build batch inform files
            foreach (var cBatch in cBatches)
                try
                {
                    // nslds-115: don't submit duplicate requests but mark them submitted
                    // load up all valid students for this batch request
                    var students = context.ClientRequestStudents
                        .Where(x => x.Link2ClientRequest == cBatch.Id && !x.IsDeleted)
                        .ToList();

                    // only send unique request types for each student
                    cBatch.Students = students
                        .OrderBy(o => o.Id)
                        .GroupBy(g => new { g.SSN, g.RequestType })
                        .Select(s => s.First())
                        .ToList();

                    if (cBatch.Students.Count == 0) { continue; }

                    // update submitted date and increment sequence#
                    seq++;
                    cBatch.SubmittedOn = DateTime.Now;
                    cBatch.Sequence = seq;

                    StringBuilder sb = BatchInformBuilder.Build(cp, cBatch);

                    var fRequest = Path.Combine(RootPath, cp.OPEID, $"{DateTime.Now.ToString("yyyy-MM-dd_HH'h'mm")}_Batch#{cBatch.Id}_Seq#{cBatch.Sequence}.txt");
                    var fFile = new StreamWriter(fRequest);
                    fFile.Write(sb.ToString());
                    fFile.Close();

                    // mark batch as submitted
                    cBatch.IsSubmitted = true;
                    // re-assign all original students to this batch
                    cBatch.Students = students;
                    context.Entry(cBatch).State = EntityState.Modified;
                    foreach (var item in students)
                    {
                        item.IsSubmitted = true;
                        item.SubmittedOn = DateTime.Now;
                        context.Entry(item).State = EntityState.Modified;
                    }
                    context.SaveChanges();
                    Logger.Information($"Created: {fRequest}");
                }
                catch (Exception ex)
                {
                    LogError($"Error in batch# {cBatch.Id} -> {ex.Message} {ex.InnerException?.Message}");
                }

            // datachk-68: build TM refresh batch request for non-reviewed expired TM responses
            var tmStudents = (from s in context.ClientRequestStudents
                              join r in context.ClientRequests on s.Link2ClientRequest equals r.Id
                              where (r.Link2ClientProfile == cp.Id && r.IsTM == true) &&
                              r.ReceivedOn.HasValue &&
                              ValidUntil(r.ReceivedOn.Value, cp.Expiration) < DateTime.Today &&
                              !s.IsRefreshed && !s.IsDeleted &&
                              (s.IsLoanReviewed == false || s.IsPellReviewed == false ||
                               s.IsGrantReviewed == false || s.IsTeachReviewed == false)
                              select s).ToList();

            if (tmStudents.Count() > 0)
            {
                // check for existing job queue available, get last job not submitted or null
                var job = context.Jobs.LastOrDefault(x =>
                  x.Link2ClientProfile == cp.Id &&
                  x.JobDate == DateTime.Today);

                // if job is null create a new one
                if (job == null)
                {
                    job = new Job
                    {
                        Link2ClientProfile = cp.Id,
                        JobDate = DateTime.Today,
                        SubmittedOn = DateTime.Now,
                        RevOn = DateTime.Now,
                        RevBy = "TdClient Scheduler"
                    };
                    context.Jobs.Add(job);
                    context.SaveChanges();
                }
                seq++;

                var newBatch = new ClientRequest
                {
                    Link2ClientProfile = cp.Id,
                    RevBy = "TdClient Scheduler",
                    RevOn = DateTime.Now,
                    SubmittedOn = DateTime.Now,
                    Response = StatusConstant.TmRefresh,
                    Sequence = seq,
                    Link2Job = job.Id
                };
                context.ClientRequests.Add(newBatch);
                context.SaveChanges();
                try
                {
                    // same query without duplicates for TM refresh
                    var newStudents = tmStudents.GroupBy(x => x.SSN).Select(s => s.First()).ToList();

                    // initialize automapper to clone student requests
                    AutoMapper.Mapper.Initialize(cfg =>
                    {
                        cfg.CreateMap<ClientRequestStudent, ClientRequestStudent>()
                        .ForMember(dest => dest.Id, opt => opt.Ignore())
                        .ForMember(dest => dest.Link2ClientRequest, opt => opt.UseValue(newBatch.Id));
                    });

                    foreach (var student in newStudents)
                    {
                        // Clone student record and add to new batch request
                        var newStudent = AutoMapper.Mapper.Map<ClientRequestStudent, ClientRequestStudent>(student);
                        newStudent.With(s =>
                        {
                            s.IsSubmitted = true;
                            s.SubmittedOn = DateTime.Now;
                            s.IsReceived = false;
                            s.ReceivedOn = null;
                            s.Response = null;
                            s.IsPellReviewed = null;
                            s.IsLoanReviewed = null;
                            s.IsGrantReviewed = null;
                            s.IsTeachReviewed = null;
                        });
                        context.ClientRequestStudents.Add(newStudent);
                    }

                    // prepare the batch inform request
                    StringBuilder sb = BatchInformBuilder.Build(cp, newBatch);

                    var fRequest = Path.Combine(RootPath, cp.OPEID, $"{DateTime.Now.ToString("yyyy-MM-dd_HH'h'mm")}_TmBatch#{newBatch.Id}_Seq#{newBatch.Sequence}.txt");
                    var fFile = new StreamWriter(fRequest);
                    fFile.Write(sb.ToString());
                    fFile.Close();

                    foreach (var student in tmStudents)
                    {
                        student.IsRefreshed = true;
                        context.Entry(student).State = EntityState.Modified;
                    }

                    newBatch.IsSubmitted = true;
                    context.Entry(newBatch).State = EntityState.Modified;
                    context.SaveChanges();

                    Logger.Information($"Created: {fRequest}");
                }
                catch (Exception ex)
                {
                    LogError($"Error in batch# {newBatch.Id} -> {ex.Message} {ex.InnerException?.Message}");
                    newBatch.Students = null;
                    newBatch.IsFailed = true;
                    context.Entry(newBatch).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }

            // check that output files exist in client folder
            if (Directory.GetFiles(clientPath, "*.txt").Count() > 0) { return true; }
            return false;
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
                Logger.Information($"NSLDS.Scheduler send job starting at: {startTime}");

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
                                    LogWarning("TD Client login failed, check error log");
                                else
                                    LogWarning("TD Client login failed, check SAIG mailbox and password settings");
                                continue;
                            }

                            // passed the login check, proceed with batch requests creation
                            Logger.Information($"Processing pending requests for {cp.Organization_Name}");
                            success = GenerateOutputFiles(cp, NsldsContext);
                            Logger.Information((success) ? $"Requests ready for {cp.Organization_Name}" : $"No new requests for {cp.Organization_Name}");

                            // run send to nslds batch command for new or pending requests
                            if (success)
                            {
                                success = ExecuteBatch("send", cp, NsldsContext);
                                Logger.Information((success) ? $"Requests sent for {cp.Organization_Name}" : $"Requests failed to send for {cp.Organization_Name}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"{item.DatabaseName} send job failed: {ex.Message} {ex.InnerException?.Message}");
                    }
                Logger.Information($"NSLDS.Scheduler send job finished at: {DateTime.Now}");
            }
            catch (Exception ex)
            {
                // only exception type that doesn't shut down the process
                // can be used in conjunction with scheduler listeners
                //throw new JobExecutionException(ex);
                LogError($"NSLDS.Scheduler send job failed: {ex.Message} {ex.InnerException?.Message}");
            }
            finally
            {
                EmailNotification();
                if (GlobalContext != null) { GlobalContext.Dispose(); }
                Logger.Information($"NSLDS.Scheduler next send job will start at: {context.Trigger.GetFireTimeAfter(DateTime.Now)}");
            }
        }
    }

    #endregion
}
