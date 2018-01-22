using System;
using System.Collections.Generic;
using System.Linq;
using NSLDS.Domain;
using System.IO;
using System.Text;
using Global.Domain;
using NSLDS.Common;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace NSLDS.API.Controllers
{
     //[Authorize]

    [Produces("application/json")]
    [Route("api/Requests")]
    public class ClientRequestController : DbContextController
    {
        #region Private Methods

        private ClientProfile _cp;

        private ClientProfile cp
        {
            get
            {
                if (_cp == null)
                {
                    _cp = NsldsContext.ClientProfiles.Single(x => x.Id == ClientProfileId);
                }
                return _cp;
            }
        }

        private bool ClientProfileExists(int id)
        {
            return NsldsContext.ClientProfiles.Any(e => e.Id == id);
        }

        private bool ClientRequestExists(int id)
        {
            return NsldsContext.ClientRequests.Any(e => e.Id == id);
        }

        private bool ValidateClientRequest(int id)
        {
            // not exist
            return !NsldsContext.ClientRequestStudents
                .Any(x => x.Link2ClientRequest == id && !x.IsDeleted && !x.IsValid);
        }

        private void SetClientRequestStatus(ClientRequest cBatch)
        {
            if (cBatch.IsDeleted)
            {
                cBatch.Status = StatusConstant.Disabled;
            }
            else if (cBatch.IsReceived && cBatch.ReceivedOn.HasValue)
            {
                // NSLDS-149: expires at midnight of expiration date
                DateTime expdate = FahDef.ValidUntil(cBatch.ReceivedOn.Value, cp.Expiration);
                // DATACHK-68: different status for TM batches
                if (cBatch.IsTM)
                {
                    if (expdate.Date < DateTime.Today) { cBatch.Status = StatusConstant.TmExpired; }
                    else
                    {
                        var isStarted = NsldsContext.ClientRequestStudents.Any(x =>
                        x.Link2ClientRequest == cBatch.Id &&
                        (x.IsGrantReviewed.Value || x.IsLoanReviewed.Value || x.IsPellReviewed.Value || x.IsTeachReviewed.Value));

                        var isComplete = !NsldsContext.ClientRequestStudents.Any(x =>
                        x.Link2ClientRequest == cBatch.Id &&
                        ((x.IsGrantReviewed == false) || (x.IsLoanReviewed == false) ||
                         (x.IsPellReviewed == false) || (x.IsTeachReviewed == false)));

                        if (isComplete) { cBatch.Status = StatusConstant.TmComplete; }
                        else if (isStarted) { cBatch.Status = StatusConstant.TmInProgress; }
                        else { cBatch.Status = StatusConstant.TmNotStarted; }
                    }
                }
                else
                {
                    if (expdate.Date < DateTime.Today) { cBatch.Status = StatusConstant.Expired; }
                    else if (cBatch.Response == StatusConstant.TmRefresh) { cBatch.Status = StatusConstant.TmRefresh; }
                    else { cBatch.Status = StatusConstant.Received; }
                }
            }
            else if (cBatch.IsFailed)
            {
                cBatch.Status = StatusConstant.Failed;
            }
            else if (cBatch.ReceivedOn.HasValue)
            {
                cBatch.Status = StatusConstant.Partial;
            }
            else if (cBatch.IsSubmitted)
            {
                cBatch.Status = StatusConstant.InProgress;
            }
            else if (cBatch.IsOnHold)
            {
                cBatch.Status = StatusConstant.OnHold;
            }
            else if (cBatch.SubmittedOn.HasValue)
            {
                cBatch.Status = StatusConstant.InQueue;
            }
            else
            {
                cBatch.Status = StatusConstant.Errors;
            }
        }

        private void SetNsldsRequestStatus(ClientRequestStudent cReq, bool disabled, bool onhold)
        {
            if (disabled)
            {
                cReq.Status = StatusConstant.Disabled;
            }
            else if (cReq.IsDeleted)
            {
                cReq.Status = StatusConstant.Deleted;
            }
            else if (cReq.IsReceived && !cReq.IsValid)
            {
                cReq.Status = StatusConstant.ReceivedErrors;
            }
            else if (cReq.IsReceived && cReq.ReceivedOn.HasValue)
            {
                // NSLDS-149: expires at midnight of expiration date
                DateTime expdate = FahDef.ValidUntil(cReq.ReceivedOn.Value, cp.Expiration);
                if (expdate.Date < DateTime.Today) { cReq.Status = StatusConstant.Expired; }
                else { cReq.Status = StatusConstant.Received; }
            }
            else if (cReq.ReceivedOn.HasValue)
            {
                cReq.Status = StatusConstant.Partial;
            }
            else if (cReq.IsSubmitted)
            {
                cReq.Status = StatusConstant.InProgress;
            }
            else if (!cReq.IsValid)
            {
                cReq.Status = StatusConstant.Errors;
            }
            else if (onhold)
            {
                cReq.Status = StatusConstant.OnHold;
            }
            else
            {
                cReq.Status = StatusConstant.InQueue;
            }
        }

        private bool ClientRequestNsldsExists(int id)
        {
            return NsldsContext.ClientRequestStudents.Any(e => e.Id == id);
        }

        private Job GetOrCreateJobForRequest(ClientRequest currentBatch)
        {
            Job job = null;

            // datachk-165: calculate next submission date/time/hour/minute (truncate seconds)
            var now = DateTime.Now;
            var dt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            DateTime nextSubmit;
            // datachk-178: remove daily upload option
            nextSubmit = dt.AddMinutes(30); // per upload, delay 30 minutes

            // check for existing job queue available, get last job not submitted or null
            job = NsldsContext.Jobs.LastOrDefault(x =>
              x.Link2ClientProfile == cp.Id &&
              x.JobDate == nextSubmit.Date);

            // if job is null create a new one
            if (job == null)
            {
                job = new Job
                {
                    Link2ClientProfile = cp.Id,
                    JobDate = nextSubmit.Date,
                    SubmittedOn = nextSubmit,
                    RevOn = DateTime.Now,
                    RevBy = UserName
                };
                NsldsContext.Jobs.Add(job);
                NsldsContext.SaveChanges();
            }

            // now update user and batch
            job.RevOn = DateTime.Now;
            job.RevBy = UserName;
            NsldsContext.Entry(job).State = EntityState.Modified;
            currentBatch.Link2Job = job.Id;
            //var sequence = NsldsContext.ClientRequests.Where(x => x.Link2Job == job.Id)?
            //    .Max(x => x.Sequence);
            //currentBatch.Sequence = (sequence != null) ? (short)(sequence + 1) : (short)1;
            currentBatch.Sequence = 0; // sequence will be set at time of posting
            currentBatch.SubmittedOn = nextSubmit;
            NsldsContext.Entry(currentBatch).State = EntityState.Modified;
            NsldsContext.SaveChanges();

            return job;
        }

        private void GenerateOutputFile(int id, bool tdclient = true)
        {
            var cBatch = NsldsContext.ClientRequests.Single(x => x.Id == id);

            // retrieve the last sequence# for today's submitted batch requests
            short seq = NsldsContext.ClientRequests
                .Where(x => x.Link2ClientProfile == cp.Id && x.SubmittedOn.HasValue &&
                    x.SubmittedOn.Value.Date == DateTime.Today && x.IsSubmitted)
                .Max(x => x.Sequence as short?) ?? 0;

            seq++;
            cBatch.Sequence = seq;

            cBatch.Students = NsldsContext.ClientRequestStudents
                .Where(x => x.Link2ClientRequest == id && !x.IsDeleted)
                .OrderBy(o => o.Id)
                .GroupBy(g => new { g.SSN, g.RequestType })
                .Select(s => s.First())
                .ToList();

            StringBuilder sb = BatchInformBuilder.Build(cp, cBatch, tdclient);

            var RootPath = HostingEnvironment.WebRootPath;
            var Date = DateTime.Today.ToString("yyyy-MM-dd");
            var fileName = $"{UserName}_Batch_{cBatch.Id}_Seq_{cBatch.Sequence:D2}.txt";
            var fRequest = Path.Combine(RootPath, OpeId, Date, fileName);

            if (!Directory.Exists(Path.GetDirectoryName(fRequest)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fRequest));
            }
            
            var fFile = new StreamWriter(fRequest);
            fFile.Write(sb.ToString());
            fFile.Close();

            // mark batch as submitted
            cBatch.IsSubmitted = true;
            cBatch.SubmittedOn = DateTime.Now;
            // store inform file web path in Response field in case user wants to retrieve it later
            cBatch.Response = $"/{OpeId}/{cBatch.SubmittedOn.Value.Date.ToString("yyyy-MM-dd")}/{Path.GetFileName(fRequest)}";

            NsldsContext.Entry(cBatch).State = EntityState.Modified;
            foreach (var item in cBatch.Students)
            {
                item.IsSubmitted = true;
                item.SubmittedOn = DateTime.Now;
                NsldsContext.Entry(item).State = EntityState.Modified;
            }
            NsldsContext.SaveChanges();
        }

        // fah alerts and codes only loaded when needed and once per session
        private List<FahAlert> _fa;
        private List<FahCode> _fc;

        private List<FahAlert> fa
        {
            get
            {
                if (_fa == null) { _fa = GlobalContext.FahAlerts.ToList(); }
                return _fa;
            }
        }

        private List<FahCode> fc
        {
            get
            {
                if (_fc == null) { _fc = GlobalContext.FahCodes.ToList(); }
                return _fc;
            }
        }

        private void ValidateFahAlerts(List<ClientRequestStudent> students)
        {
            // load type1 & type5 alert condition records in bulk
            var sids = students.Select(x => x.Id).ToList();
            var type1s = NsldsContext.FahType1Recs
                .Where(x => sids.Contains(x.Link2ClientRequestStudent)).ToList();
            var type5s = NsldsContext.FahType5Recs.Where(x =>
                sids.Contains(x.Link2ClientRequestStudent) &&
                ((x.LoanStatusCode == FahDef.IAStatus && x.TotalDisb == 0) ||
                (x.LoanStatusCode == FahDef.VAStatus) ||
                (FahDef.loanFlags.Contains(x.AdditionalUnsubLoan)) ||
                (x.Reaffirmation == true)))
                .ToList();
            // load type3 grant flag alert conditions in bulk
            var type3s = NsldsContext.FahType3Recs
                .Where(x => sids.Contains(x.Link2ClientRequestStudent) &&
                FahDef.grantFlags.Contains(x.Overpayment)).ToList();

            // loop through type1 records -> students with fah data
            foreach (var type1 in type1s)
            {
                var student = students.SingleOrDefault(x => x.Id == type1.Link2ClientRequestStudent);
                if (student == null) { continue; } // unlikely, just in case
                var loans = type5s.Where(x => x.Link2ClientRequestStudent == student.Id).ToList();
                var grantflags = type3s.Where(x => x.Link2ClientRequestStudent == student.Id).ToList();
                CalcFahAlerts(type1, loans, student, grantflags);
            }
        }

        private void ValidateFahAlerts(ClientRequestStudent student)
        {
            var type1 = NsldsContext.FahType1Recs
                .FirstOrDefault(x => x.Link2ClientRequestStudent == student.Id);
            if (type1 == null) { return; } // no FAH data received

            var loans = NsldsContext.FahType5Recs.Where(x =>
                x.Link2ClientRequestStudent == student.Id &&
                ((x.LoanStatusCode == FahDef.IAStatus && x.TotalDisb == 0) ||
                (x.LoanStatusCode == FahDef.VAStatus) ||
                (FahDef.loanFlags.Contains(x.AdditionalUnsubLoan)) ||
                (x.Reaffirmation == true))).ToList();

            var grantflags = NsldsContext.FahType3Recs
                .Where(x => x.Link2ClientRequestStudent == student.Id &&
                FahDef.grantFlags.Contains(x.Overpayment)).ToList();

            CalcFahAlerts(type1, loans, student, grantflags);
        }

        private void CalcFahAlerts(NsldsFAHType1 type1, List<NsldsFAHType5> loans, ClientRequestStudent student, List<NsldsFAHType3> grantflags)
        {
            bool alert1 = loans.Any(x => x.LoanStatusCode == FahDef.IAStatus && x.TotalDisb == 0);
            bool alert2 = type1.Fraud == true;
            bool alert3 = type1.ActiveBankruptcy == true;
            bool alert4 = FahDef.loanCodesDischarged.Contains(type1.DischargedLoanCode);
            bool alert5 = type1.DefaultedLoan == true;
            bool alert6 = loans.Any(x => FahDef.loanFlags.Contains(x.AdditionalUnsubLoan));
            bool alert7 = loans.Any(x => x.Reaffirmation == true);
            // datachk-161: VA loan discharged status added to alerts
            bool alert8 = loans.Any(x => x.LoanStatusCode == FahDef.VAStatus);
            bool alert9 = FahDef.loanCodes.Contains(type1.UnderGradSubLoanLimit);
            bool alert10 = FahDef.loanCodes.Contains(type1.UnderGradCombinedLoanLimit)
                && type1.UndergradDependency == FahDef.depStatus;
            bool alert11 = FahDef.loanCodes.Contains(type1.UnderGradCombinedLoanLimit)
                && type1.UndergradDependency == FahDef.indStatus;
            bool alert12 = FahDef.loanCodes.Contains(type1.GradCombinedLoanLimit);
            bool alert13 = FahDef.loanCodes.Contains(type1.GradSubLoanLimit);
            bool alert14 = type1.SulaFlag == true;
            bool alert15 = type1.TeachGrantConverted == true;
            bool alert16 = FahDef.leuCodes.Contains(type1.PellLifeLimit);
            bool alertgrants = grantflags.Count > 0;

            // if no fah alerts present, no further processing needed
            if (!alert1 && !alert2 && !alert3 && !alert4 && !alert5 && !alert6 && !alert7 && !alert8 && !alert9 & !alert10 && !alert11 && !alert12 && !alert13 && !alert14 && !alert15 && !alert16 && !alertgrants) { return; }

            // initialize a dictionary
            var fahAlerts = new Dictionary<string, string>();

            if (alert1)
            {
                var loan1 = loans.First(x => x.LoanStatusCode == FahDef.IAStatus && x.TotalDisb == 0);
                fahAlerts.Add(fa.Single(x => x.Id == FahDef.a1).Name, loan1.CodeName(x => x.LoanStatusCode, fc));
            }
            if (alert2)
            {
                fahAlerts.Add(fa.Single(x => x.Id == FahDef.a2).Name, type1.CodeName(x => x.Fraud, fc));
            }
            if (alert3)
            {
                fahAlerts.Add(fa.Single(x => x.Id == FahDef.a3).Name, type1.CodeName(x => x.ActiveBankruptcy, fc));
            }
            if (alert4)
            {
                fahAlerts.Add(fa.Single(x => x.Id == FahDef.a4).Name, type1.CodeName(x => x.DischargedLoanCode, fc));
            }
            if (alert5)
            {
                fahAlerts.Add(fa.Single(x => x.Id == FahDef.a5).Name, type1.CodeName(x => x.DefaultedLoan, fc));
            }
            if (alert6)
            {
                var loan6s = loans.Where(x => FahDef.loanFlags.Contains(x.AdditionalUnsubLoan))
                    .GroupBy(x => x.AdditionalUnsubLoan).Select(f => f.First()).ToList();
                var loan6codes = new List<string>();
                foreach (var item in loan6s)
                {
                    loan6codes.Add(item.CodeName(x => x.AdditionalUnsubLoan, fc));
                }
                fahAlerts.Add(fa.Single(x => x.Id == FahDef.a6).Name, string.Join(", ", loan6codes));
            }
            if (alert7)
            {
                var loan7 = loans.First(x => x.Reaffirmation == true);
                fahAlerts.Add(fa.Single(x => x.Id == FahDef.a7).Name, loan7.CodeName(x => x.Reaffirmation, fc));
            }
            // datachk-161: VA loan discharged status added to alerts
            if (alert8)
            {
                var loan8 = loans.First(x => x.LoanStatusCode == FahDef.VAStatus);
                fahAlerts.Add(fa.Single(x => x.Id == FahDef.a8).Name, loan8.CodeName(x => x.LoanStatusCode, fc));
            }
            if (alert9)
            {
                fahAlerts.Add(fa.Single(x => x.Id == FahDef.a9).Name, type1.CodeName(x => x.UnderGradSubLoanLimit, fc));
            }
            if (alert10)
            {
                fahAlerts.Add(fa.Single(x => x.Id == FahDef.a10).Name, type1.CodeName(x => x.UnderGradCombinedLoanLimit, fc));
            }
            if (alert11)
            {
                fahAlerts.Add(fa.Single(x => x.Id == FahDef.a11).Name, type1.CodeName(x => x.UnderGradCombinedLoanLimit, fc));
            }
            if (alert12)
            {
                fahAlerts.Add(fa.Single(x => x.Id == FahDef.a12).Name, type1.CodeName(x => x.GradCombinedLoanLimit, fc));
            }
            if (alert13)
            {
                fahAlerts.Add(fa.Single(x => x.Id == FahDef.a13).Name, type1.CodeName(x => x.GradSubLoanLimit, fc));
            }
            if (alert14)
            {
                fahAlerts.Add(fa.Single(x => x.Id == FahDef.a14).Name, type1.CodeName(x => x.SulaFlag, fc));
            }
            if (alert15)
            {
                fahAlerts.Add(fa.Single(x => x.Id == FahDef.a15).Name, type1.CodeName(x => x.TeachGrantConverted, fc));
            }
            if (alert16)
            {
                fahAlerts.Add(fa.Single(x => x.Id == FahDef.a16).Name, type1.CodeName(x => x.PellLifeLimit, fc));
            }
            if (alertgrants)
            {
                foreach (var item in grantflags)
                {
                    fahAlerts.Add(
                        item.CodeName(x => x.OverpaymentType, fc), 
                        item.CodeName(x => x.Overpayment, fc));
                }
            }

            if (fahAlerts.Count > 0) { student.FahAlerts = fahAlerts; }
        }

        #endregion

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                NsldsContext.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Constructor

        public ClientRequestController(GlobalContext globalContext, IHostingEnvironment hostingEnvironment, IConfiguration configuration, IRuntimeOptions runtimeOptions, ILogger<ClientRequestController> logger)
            : base(globalContext, hostingEnvironment, configuration, runtimeOptions, logger)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get all batch requests for current client with optional parameters
        /// </summary>
        /// <param name="batchid"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <param name="isdeleted"></param>
        /// <param name="issubmitted"></param>
        /// <param name="isfailed"></param>
        /// <param name="isvalid"></param>
        /// <param name="inqueue"></param>
        /// <param name="isreceived"></param>
        /// <param name="isexpired"></param>
        /// <param name="istm"></param>
        /// <returns></returns>
        // GET: api/Requests
      //  [Authorize(Policy = "FileReview")]
        [HttpGet]
        public IActionResult GetClientRequests([FromQuery] int? batchid, [FromQuery] DateTime? startdate, [FromQuery] DateTime? enddate, [FromQuery] bool? isdeleted, [FromQuery] bool? issubmitted, [FromQuery] bool? isfailed, [FromQuery] bool? isvalid, [FromQuery] bool? inqueue, [FromQuery] bool? isreceived, [FromQuery] bool? isexpired, [FromQuery] bool? istm)
        {
            if (ClientProfileId != 0)
            {
                // if dates not provided use client profile expiration range
                if (!startdate.HasValue) { startdate = FahDef.ValidFrom(DateTime.Today, cp.Expiration); };
                if (!enddate.HasValue) { enddate = DateTime.Now; };

                var clientRequests =
                    (batchid.HasValue) ? // optional batchid determines the query
                    NsldsContext.ClientRequests.Include(x => x.Alerts).Where(
                        m => m.Link2ClientProfile == ClientProfileId
                        && m.Id == batchid).ToList() : // no batchid parameter
                    NsldsContext.ClientRequests.Include(x => x.Alerts).Where(
                        m => m.Link2ClientProfile == ClientProfileId
                        && m.RevOn.Value.Date >= startdate
                        && m.RevOn.Value.Date <= enddate)
                        .OrderByDescending(m => m.RevOn).ToList();

                foreach (var item in clientRequests)
                {
                    item.IsValid = (item.IsSubmitted) ? true : ValidateClientRequest(item.Id);
                    SetClientRequestStatus(item);
                    item.RouteData = string.Format(RouteConstant.Request, item.Id);
                }

                // further filtering on optional parameters if batchId is null
                if (!batchid.HasValue)
                {
                    if (isdeleted.HasValue) { clientRequests = clientRequests.Where(m => m.IsDeleted == isdeleted).ToList(); }
                    if (issubmitted.HasValue) { clientRequests = clientRequests.Where(m => m.IsSubmitted == issubmitted).ToList(); }
                    if (isfailed.HasValue) { clientRequests = clientRequests.Where(m => m.IsFailed == isfailed).ToList(); }
                    if (isvalid.HasValue) { clientRequests = clientRequests.Where(m => m.IsValid == isvalid).ToList(); }
                    if (inqueue.HasValue)
                    {
                        clientRequests = clientRequests
                        .Where(m => (inqueue.Value) ? m.Status == StatusConstant.InQueue : m.Status != StatusConstant.InQueue).ToList();
                    }
                    if (isreceived.HasValue) { clientRequests = clientRequests.Where(m => m.IsReceived == isreceived).ToList(); }
                    if (isexpired.HasValue)
                    {
                        clientRequests = clientRequests
                        .Where(m => (isexpired.Value) ? m.Status == StatusConstant.Expired : m.Status != StatusConstant.Expired).ToList();
                    }
                    // datachk-68 allow filtering by TM batch status (optional)
                    if (istm.HasValue) { clientRequests = clientRequests.Where(m => m.IsTM == istm.Value).ToList(); }
                }

                var result = new ClientRequestsResult
                {
                    Count = clientRequests.Count(),
                    Results = clientRequests
                };

                return Ok(result);

            }
            else
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }
        }

        /// <summary>
        /// Get nslds records by batch "id"
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/Requests/4
       // [Authorize(Policy = "FileReview")]
        [HttpGet("{id}", Name = "GetNsldsRequestsById")]
        public IActionResult GetNsldsRequestsById([FromRoute] int id)
        {
            if (ClientProfileId == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }

            if (ClientRequestExists(id))
            {
                // test validation of the current batch of student requests
                var currentBatch = NsldsContext.ClientRequests.Single(x => x.Id == id);
                SetClientRequestStatus(currentBatch);
                var isValid = (currentBatch.IsSubmitted) ? true : ValidateClientRequest(id);

                // retrieve students for this batch request
                var clientRequestStudents = NsldsContext.ClientRequestStudents
                    .Include(x => x.Alerts)
                     .Where(m => m.Link2ClientRequest == id).ToList();

                foreach (var item in clientRequestStudents)
                {
                    if (!item.IsSubmitted) { NsldsRequestValidator.ValidateRequest(item); }
                    item.RouteData = string.Format(RouteConstant.Student, item.Id);
                    SetNsldsRequestStatus(item, currentBatch.IsDeleted, currentBatch.IsOnHold);
                }

                if (currentBatch.IsReceived)
                {
                    ValidateFahAlerts(clientRequestStudents);
                }

                return Ok(new ClientNsldsRequestsResult
                {
                    BatchId = id,
                    Count = clientRequestStudents.Count(),
                    IsValid = isValid,
                    IsOnHold = currentBatch.IsOnHold,
                    Status = currentBatch.Status,
                    RouteData = string.Format(RouteConstant.Request, id),
                    // RouteSubmit = (isValid) ? string.Format(RouteConstant.Queue, id) : null,
                    Results = clientRequestStudents
                });

            }
            else
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidBatchId });
            }
        }

        /// <summary>
        /// Save modified batch record
        /// </summary>
        /// <param name="id"></param>
        /// <param name="clientRequest"></param>
        /// <returns></returns>
        // PUT: api/Requests/5
      //  [Authorize(Policy = "Editor")]
        [HttpPut("{id}")]
        public IActionResult PutClientRequest([FromRoute] int id, [FromBody] ClientRequest clientRequest)
        {
            if (ClientProfileId == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }

            if (id != clientRequest.Id)
            {
                return BadRequest(new ErrorResult { Message = ErrorConstant.InvalidBatchId });
            }

            if (!ClientRequestExists(id))
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidBatchId });
            }

            // check if existing batchRequest is already submitted
            var issubmitted = NsldsContext.ClientRequests
                .Any(x => x.Id == id && x.IsSubmitted);

            // check that batch is not already submitted
            if (issubmitted)
            {
                return BadRequest(new ErrorResult { Message = ErrorConstant.BatchAlreadySubmitted });
            }

            //nsldsRequest.RevOn = DateTime.Now;
            clientRequest.RevBy = UserName;

            // if a deletion or on hold, remove from job
            if ((clientRequest.IsOnHold || clientRequest.IsDeleted) && clientRequest.Link2Job != null)
            {
                clientRequest.Link2Job = null;
                clientRequest.Sequence = 0;
                clientRequest.SubmittedOn = null;
            }

            NsldsContext.Entry(clientRequest).State = EntityState.Modified;

            NsldsContext.SaveChanges();

            var isValid = ValidateClientRequest(clientRequest.Id);
            clientRequest.IsValid = isValid;

            // check if batch request needs to go to queue
            if (isValid && !clientRequest.IsDeleted && !clientRequest.IsOnHold && clientRequest.Link2Job == null)
            {
                var job = GetOrCreateJobForRequest(clientRequest);
            }

            SetClientRequestStatus(clientRequest);

            return Ok(new ClientRequestResult
            {
                IsValid = isValid,
                RouteData = string.Format(RouteConstant.Request, clientRequest.Id),
                RouteSubmit = (isValid) ? string.Format(RouteConstant.Queue, clientRequest.Id) : null,
                Result = clientRequest
            });
        }

        /// <summary>
        /// Get multiple nslds record by ssn, student name, dob 
        /// </summary>
        /// <param name="ssn"></param>
        /// <param name="name"></param>
        /// <param name="dob"></param>
        /// <param name="status"></param>
        /// <param name="sula"></param>
        /// <param name="openay"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <param name="batchid"></param>
        /// <returns></returns>
        // GET: api/Requests/Student?ssn=
     //   [Authorize(Policy = "FileReview")]
        [HttpGet("Student", Name = "GetNsldsStudent")]
        public IActionResult GetNsldsStudent([FromQuery] string ssn, [FromQuery] string name, [FromQuery] DateTime? dob, [FromQuery] string status, [FromQuery] bool? sula, [FromQuery] bool? openay, [FromQuery] DateTime? startdate, [FromQuery] DateTime? enddate, [FromQuery] int? batchid)
        {
            if (ClientProfileId == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }

            // if dates not provided use client profile expiration range
            if (!startdate.HasValue) { startdate = FahDef.ValidFrom(DateTime.Today, cp.Expiration); };
            if (!enddate.HasValue) { enddate = DateTime.Now; };

            string name1 = null, name2 = null, name3 = null;
            // if name param is not null, override first & last
            if (name != null)
            {
                // check if there's whitespace to split the string
                var names = name.Split();
                name1 = (names.Length > 0) ? names[0].ToUpper() : null;
                name2 = (names.Length > 1) ? names[1].ToUpper() : null;
                name3 = (names.Length > 2) ? names[2].ToUpper() : null;
            }

            // restrict the student search to received fah reponses
            bool hasfah = (status != null || sula != null || openay != null);

            // initialize the searchresult object
            var result = new StudentSearchResult { Count = 0 };

            // parameters for SP have to be in same order as corresponding SqlConstant
            var parms = new object[]
            {
                ClientProfileId,
                startdate,
                enddate,
                Encryption._encrypt(ssn),
                name1,
                name2,
                name3,
                dob,
                openay,
                hasfah
            };
            // main student search query
            var query = NsldsContext.Set<StudentSearchDetail>()
                .FromSql(SqlConstant.StudentSearchSP, parms).ToList();

            // nslds-150: optional query parameter batchid
            if (batchid.HasValue)
            {
                query = query.Where(x => x.BatchId == batchid.Value).ToList();
            }

            // get a list of student IDs from search query result if additional filtering
            List<int> sids = (hasfah) ? query.Select(x => x.StudentId).ToList() : null;

            // if 'status' parameter is not null, additional search needed
            List<int> statuslist = null;
            if (status != null)
            {
                statuslist = new List<int>();

                status = status.ToUpper();
                bool overpay = status.Contains(FahDef.Overpay);
                bool def = status.Contains(FahDef.Default);
                bool death = status.Contains(FahDef.Death);
                bool fraud = status.Contains(FahDef.Fraud);
                bool discharged = status.Contains(FahDef.Discharged);
                bool other = def || death || fraud || discharged;

                if (overpay)
                {
                    // retrieve FAHtype1 records and check for loan overpayment flags
                    var type1 = NsldsContext.FahType1Recs
                        .Where(x => sids.Contains(x.Link2ClientRequestStudent)).ToList();

                    foreach (var item in type1)
                    {
                        //var agg = new LoanAggregateLimitCalc(item);
                        //if (agg.totaldependent < 0 || agg.totalindependent < 0 || agg.gradtotal < 0)
                        if (FahDef.HasLoanOverpayment(item))
                        { statuslist.Add(item.Link2ClientRequestStudent); }
                    }
                }
                if (other)
                {
                    statuslist.AddRange(
                        (from s in NsldsContext.FahType5Recs
                         where (sids.Contains(s.Link2ClientRequestStudent)) &&
                         ((def && FahDef.loanDefault.Contains(s.LoanStatusCode)) ||
                         (death && FahDef.loanDeath.Contains(s.LoanStatusCode)) ||
                         (fraud && FahDef.loanFraud.Contains(s.LoanStatusCode)) ||
                         (discharged && FahDef.loanDischarged.Contains(s.LoanStatusCode)))
                         select s.Link2ClientRequestStudent).Distinct().ToList()
                         );
                }
            }

            if (statuslist != null)
            {
                // update query and sids list
                query = query.Where(x => statuslist.Contains(x.StudentId)).ToList();
                sids = query.Select(x => x.StudentId).ToList();
            }
            
            // if sula parameter used build list
            List<int> sulalist = null;
            if (sula.HasValue)
            {
                // retrieve FAHtype1 records with SULA values
                sulalist = (from s in NsldsContext.FahType1Recs
                            where (sids.Contains(s.Link2ClientRequestStudent))
                            && s.SulaFlag == sula.Value
                            select s.Link2ClientRequestStudent).ToList();
            }

            if (sulalist != null)
            {
                // update query and sids list
                query = query.Where(x => sulalist.Contains(x.StudentId)).ToList();
                sids = query.Select(x => x.StudentId).ToList();
            }

            // if openay parameter used check openay flag for each student
            if (openay.HasValue)
            {
                var loans = (from l in NsldsContext.FahType5Recs
                             join s in NsldsContext.ClientRequestStudents
                             on l.Link2ClientRequestStudent equals s.Id
                             where sids.Contains(l.Link2ClientRequestStudent)
                               && s.StartDate <= (l.AYEndDate ?? l.LoanPeriodEndDate)
                               && FahDef.loanTypes.Contains(l.LoanTypeCode)
                               && l.LoanStatusCode != FahDef.CAStatus
                             select l).ToList();

                var ayweeks = cp.AY_Definition;

                foreach (var item in query.ToList()) // use a copy of the querylist as reference
                {
                    var type5 = loans.Where(x => x.Link2ClientRequestStudent == item.StudentId).ToList();

                    // calc open AY
                    var openAY = FahDef.CalcOpenAY(item.StartDate, type5, ayweeks);

                    if ((openay.Value && openAY == FahDef.N) ||
                        (!openay.Value && openAY == FahDef.Y)) { query.Remove(item); }
                }
            }

            // assign search results
            result.Results = query;
            result.Count = query.Count();

            return Ok(result);
        }

        /// <summary>
        /// Get single nslds record by "id"
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/Requests/Student/4
      //  [Authorize(Policy = "FileReview")]
        [HttpGet("Student/{id}", Name = "GetNsldsStudentById")]
        public IActionResult GetNsldsStudentById([FromRoute] int id)
        {
            if (ClientProfileId == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }

            if (ClientRequestNsldsExists(id))
            {
                ClientRequestStudent clientRequestStudent =
                    NsldsContext.ClientRequestStudents.Include(x => x.Alerts).Single(m => m.Id == id);

                clientRequestStudent.RouteData = string.Format(RouteConstant.Student, id);
                NsldsRequestValidator.ValidateRequest(clientRequestStudent);

                // test validation of the current batch for this student request
                var batchId = clientRequestStudent.Link2ClientRequest;
                var isValid = ValidateClientRequest(batchId);
                var cBatch = NsldsContext.ClientRequests.Single(x => x.Id == batchId);

                SetClientRequestStatus(cBatch);
                SetNsldsRequestStatus(clientRequestStudent, cBatch.IsDeleted, cBatch.IsOnHold);

                if (clientRequestStudent.IsReceived) { ValidateFahAlerts(clientRequestStudent); }

                return Ok(new ClientNsldsRequestResult
                {
                    BatchId = batchId,
                    IsValid = isValid,
                    Status = cBatch.Status,
                    RouteData = string.Format(RouteConstant.Request, batchId),
                    // RouteSubmit = (isValid) ? string.Format(RouteConstant.Queue, batchId) : null,
                    Result = clientRequestStudent
                });
            }
            else
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidNsldsRequest });
            }
        }

        /// <summary>
        /// Get nslds record history by "id"
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/Requests/Student/4/History
      //  [Authorize(Policy = "Editor")]
        [HttpGet("Student/{id}/History", Name = "GetNsldsStudentHistoryById")]
        public IActionResult GetNsldsStudentHistoryById([FromRoute] int id)
        {
            if (ClientProfileId == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }

            var history = NsldsContext.ClientRequestStudents_History
                .Where(x => x.Link2ClientRequestStudent == id)
                .OrderByDescending(x => x.ActionOn);

            return Ok(history);
        }

        /// <summary>
        /// Save modified nslds record
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nsldsRequest"></param>
        /// <returns></returns>
        // PUT: api/Requests/Student/5
      //  [Authorize(Policy = "Editor")]
        [HttpPut("Student/{id}")]
        public IActionResult PutNsldsRequest([FromRoute] int id, [FromBody] ClientRequestStudent nsldsRequest)
        {
            if (ClientProfileId == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }

            if (id != nsldsRequest.Id)
            {
                return BadRequest(new ErrorResult { Message = ErrorConstant.InvalidNsldsRequest });
            }

            if (!ClientRequestNsldsExists(id))
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.NsldsRequestNotFound });
            }

            // check that batch id is valid
            int batchId = nsldsRequest.Link2ClientRequest;

            if (!ClientRequestExists(batchId))
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidBatchId });
            }

            // retrieve existing batchRequest
            ClientRequest batchRequest = NsldsContext.ClientRequests.Single(x => x.Id == batchId);

            // check that batch is not already submitted
            if (batchRequest.IsSubmitted && !batchRequest.IsReceived)
            {
                return BadRequest(new ErrorResult { Message = ErrorConstant.BatchAlreadySubmitted });
            }

            // retrieve previous request record for history table
            var oldRec = NsldsContext.ClientRequestStudents.Single(x => x.Id == id);

            // check for previous history record & detach the old record from context
            var exist = NsldsContext.ClientRequestStudents_History.Any(x => x.Link2ClientRequestStudent == id);
            NsldsContext.Entry(oldRec).State = EntityState.Detached;

            // Initialize Automapper engine
            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<ClientRequestStudent, ClientRequestStudent_History>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Link2ClientRequestStudent, opt => opt.MapFrom(src => src.Id));
            });
            // Convert old to history record
            var hisRec = AutoMapper.Mapper.Map<ClientRequestStudent, ClientRequestStudent_History>(oldRec);
            hisRec.Action = (exist) ? "Updated" : "New";
            hisRec.ActionOn = DateTime.Now;
            hisRec.ActionBy = UserName;
            NsldsContext.ClientRequestStudents_History.Add(hisRec);

            // datachk-68: update review alerts and start date only when batch is received
            ClientRequestStudent updatedRec = (batchRequest.IsReceived) ? oldRec : nsldsRequest;
            if (batchRequest.IsReceived)
            {
                updatedRec.With(x =>
                {
                    x.StartDate = nsldsRequest.StartDate;
                    x.IsPellReviewed = nsldsRequest.IsPellReviewed;
                    x.IsLoanReviewed = nsldsRequest.IsLoanReviewed;
                    x.IsGrantReviewed = nsldsRequest.IsGrantReviewed;
                    x.IsTeachReviewed = nsldsRequest.IsTeachReviewed;
                });
                // mark student refreshed if all alerts are reviewed
                updatedRec.IsRefreshed =
                    (!updatedRec.IsGrantReviewed.HasValue || updatedRec.IsGrantReviewed.Value) &&
                    (!updatedRec.IsLoanReviewed.HasValue || updatedRec.IsLoanReviewed.Value) &&
                    (!updatedRec.IsPellReviewed.HasValue || updatedRec.IsPellReviewed.Value) &&
                    (!updatedRec.IsTeachReviewed.HasValue || updatedRec.IsTeachReviewed.Value);
            }

            // prepare to save the modified nslds request
            updatedRec.RevOn = DateTime.Now;
            updatedRec.RevBy = UserName;

            // validate the nslds request
            if (!batchRequest.IsReceived) { NsldsRequestValidator.ValidateRequest(updatedRec); }

            NsldsContext.Entry(updatedRec).State = EntityState.Modified;

            NsldsContext.SaveChanges();

            // test validation of the current batch of student requests
            bool isValid = ValidateClientRequest(batchId);

            if (!batchRequest.IsReceived)
            {
                // if batch request is already in queue and no longer valid, remove from queue
                if (!isValid && batchRequest.Link2Job != null)
                {
                    batchRequest.Link2Job = null;
                    batchRequest.Sequence = 0;
                    batchRequest.SubmittedOn = null;
                    NsldsContext.Entry(batchRequest).State = EntityState.Modified;
                    NsldsContext.SaveChanges();
                }
                else if (isValid && !batchRequest.IsOnHold && batchRequest.Link2Job == null)
                {
                    var job = GetOrCreateJobForRequest(batchRequest);
                }
            }

            // add the route data to this updated student record
            updatedRec.RouteData = string.Format(RouteConstant.Student, updatedRec.Id);
            SetNsldsRequestStatus(updatedRec, batchRequest.IsDeleted, batchRequest.IsOnHold);

            // set the status of the batch
            SetClientRequestStatus(batchRequest);
            // if student data is received, add the Fah alerts
            if (updatedRec.IsReceived) { ValidateFahAlerts(updatedRec); }

            var result = new ClientNsldsRequestResult
            {
                BatchId = batchId,
                IsValid = isValid,
                Status = batchRequest.Status,
                RouteData = string.Format(RouteConstant.Request, batchId),
                RouteSubmit = (isValid) ? string.Format(RouteConstant.Queue, batchId) : null,
                Result = updatedRec
            };
            return Ok(result);
        }

        /// <summary>
        /// Save new NSLDS student requests with optional batchId parameter
        /// </summary>
        /// <param name="Students"></param>
        /// <param name="batchId"></param>
        /// <returns></returns>
        // POST: api/Requests/Student
      //  [Authorize(Policy = "Editor")]
        [HttpPost("Student")]
        public IActionResult PostNsldsRequest([FromBody] IEnumerable<ClientRequestStudent> Students, [FromQuery] int batchId = 0)
        {
            if (ClientProfileId == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }

            if (Students == null || Students?.Count() == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.BatchHasNoRecords });
            }

            ClientRequest batchRequest;

            // existing batchId or new batch if 0
            if (batchId == 0)
            {
                batchRequest = new ClientRequest
                {
                    Link2ClientProfile = ClientProfileId,
                    IsDeleted = false,
                    IsReceived = false,
                    IsSubmitted = false,
                    RevOn = DateTime.Now,
                    RevBy = UserName
                };
                NsldsContext.ClientRequests.Add(batchRequest);
                NsldsContext.SaveChanges();
                batchId = batchRequest.Id;
            }
            else
            {
                if (!ClientRequestExists(batchId))
                {
                    return NotFound(new ErrorResult { Message = ErrorConstant.InvalidBatchId });
                }
                // retrieve existing batchRequest
                ClientRequest req = NsldsContext.ClientRequests.Single(x => x.Id == batchId);

                // check that batch is not already submitted
                if (req.IsSubmitted || req.SubmittedOn <= DateTime.Now)
                {
                    return BadRequest(new ErrorResult { Message = ErrorConstant.BatchAlreadySubmitted });
                }
            }

            foreach (ClientRequestStudent nsldsRequest in Students)
            {
                nsldsRequest.Link2ClientRequest = batchId;
                nsldsRequest.IsDeleted = false;
                nsldsRequest.IsReceived = false;
                nsldsRequest.IsSubmitted = false;
                nsldsRequest.RevOn = DateTime.Now;
                nsldsRequest.RevBy = UserName;
                // validate the nsldsRequest
                NsldsRequestValidator.ValidateRequest(nsldsRequest);
            }

            // add new students to the batch request
            NsldsContext.ClientRequestStudents.AddRange(Students);
            NsldsContext.SaveChanges();

            // test validation of a batch of student requests
            bool isValid = ValidateClientRequest(batchId);

            // retrieve updated batchRequest including all students
            batchRequest = NsldsContext.ClientRequests
                .Include(x => x.Students).Single(x => x.Id == batchId);

            // if batch request is already in queue and no longer valid, remove from queue
            if (!isValid && batchRequest.Link2Job != null)
            {
                batchRequest.Link2Job = null;
                batchRequest.Sequence = 0;
                batchRequest.SubmittedOn = null;
                NsldsContext.Entry(batchRequest).State = EntityState.Modified;
                NsldsContext.SaveChanges();
            }
            else if (isValid && !batchRequest.IsOnHold && batchRequest.Link2Job == null)
            {
                var job = GetOrCreateJobForRequest(batchRequest);
            }

            // set the batch request status
            SetClientRequestStatus(batchRequest);

            // add the route data to all student records in this batch request
            foreach (var item in batchRequest.Students)
            {
                SetNsldsRequestStatus(item, batchRequest.IsDeleted, batchRequest.IsOnHold);
                item.RouteData = string.Format(RouteConstant.Student, item.Id);
            }

            var result = new ClientNsldsRequestsResult
            {
                BatchId = batchId,
                Count = batchRequest.Students.Count,
                IsValid = isValid,
                IsOnHold = batchRequest.IsOnHold,
                Status = batchRequest.Status,
                RouteData = string.Format(RouteConstant.Request, batchId),
                RouteSubmit = (isValid) ? string.Format(RouteConstant.Queue, batchId) : null,
                Results = batchRequest.Students
            };

            return Ok(result);
        }

        /// <summary>
        /// Upload and process Excel files containing new NSLDS requests
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        // POST: api/Requests/Upload
     //   [Authorize(Policy = "Editor")]
        [HttpPost("Upload")]
        public IActionResult Upload(IEnumerable<IFormFile> files, [FromServices] FileImportProcessor FileImportProcessor)
        {
            if (ClientProfileId == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }

            if (files == null || files?.Count() == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.FileHasNoValidRecords });
            }

            var results = new FilesUploadResult { Count = files.Count() };

            // loop through the file collection

            foreach (var file in files)
            {
                var result = new FileUploadResult();
                int batchId = 0;
                bool isValid = false;
                string batchStatus = string.Empty;
                try
                {
                    // process the incoming file
                    bool success = FileImportProcessor.Process(User, file);

                    if (!success) { continue; }

                    // create new client request and add student records from fileprocessor
                    ClientRequest newBatch = new ClientRequest
                    {
                        Link2ClientProfile = ClientProfileId,
                        IsDeleted = false,
                        IsReceived = false,
                        IsSubmitted = false,
                        RevOn = DateTime.Now,
                        RevBy = UserName
                    };

                    // datachk-174: don't save batch until all students are validated
                    foreach (ClientRequestStudent nsldsRequest in FileImportProcessor.StudentRequests)
                    {
                        nsldsRequest.Link2ClientRequest = batchId;
                        nsldsRequest.RevOn = DateTime.Now;
                        nsldsRequest.RevBy = UserName;
                        nsldsRequest.IsDeleted = false;
                        nsldsRequest.IsReceived = false;
                        nsldsRequest.IsSubmitted = false;
                        nsldsRequest.IsValid = false;
                        // validate the nsldsRequest
                        NsldsRequestValidator.ValidateRequest(nsldsRequest);
                    }

                    newBatch.Students = FileImportProcessor.StudentRequests;
                    NsldsContext.ClientRequests.Add(newBatch);

                    // save new student requests to database
                    NsldsContext.SaveChanges();
                    batchId = newBatch.Id;

                    // validate the new batch request
                    isValid = ValidateClientRequest(batchId);
                    // add to job queue if valid
                    if (isValid)
                    {
                        var job = GetOrCreateJobForRequest(newBatch);
                    }

                    // set batch status
                    SetClientRequestStatus(newBatch);
                    batchStatus = newBatch.Status;
                }
                catch
                {
                    FileImportProcessor.ErrorMessage = ErrorConstant.InvalidFile;
                }
                finally
                {
                    result.FileName = FileImportProcessor.FileName;
                    result.BatchId = batchId;
                    result.IsValid = isValid;
                    result.Status = batchStatus;
                    result.RouteData = (batchId > 0) ? string.Format(RouteConstant.Request, batchId) : null;
                    result.Message = FileImportProcessor.ErrorMessage;
                    results.Results.Add(result);
                }
            }

            return Ok(results);
        }

        /// <summary>
        /// View the job queue pending
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        // GET: api/Requests/Queue
      //  [Authorize(Policy = "Editor")]
        //[AllowAnonymous]
        [HttpGet("Queue", Name = "ViewNsldsQueue")]
        public IActionResult ViewNsldsQueue()
        {
            if (ClientProfileId == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }

            IEnumerable<Job> jobs = NsldsContext.Jobs.Include(x => x.ClientRequests)
                .Where(x => x.Link2ClientProfile == ClientProfileId && !x.IsSubmitted);

            foreach (var job in jobs)
            {
                job.RouteSubmit = string.Format(RouteConstant.Submit, job.Id);

                foreach (var batch in job.ClientRequests)
                {
                    batch.IsValid = true;
                    SetClientRequestStatus(batch);
                    batch.RouteData = string.Format(RouteConstant.Request, batch.Id);
                }
            }

            var result = new ClientJobsResult
            {
                Count = jobs.Count(),
                Results = jobs
            };

            return Ok(result);
        }

        /// <summary>
        /// Export a batch request to Excel spreadsheet
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/Requests/Export/batchid
      //  [Authorize(Policy = "Editor")]
        [HttpGet("Export/{id}", Name = "ExportBatchRequest")]
        public IActionResult ExportBatchRequest([FromRoute] int id, [FromServices] FileExportProcessor fileExportProcessor)
        {
            if (ClientProfileId == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }
            if (!ClientRequestExists(id))
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidBatchId });
            }

            // load the export processor with non-deleted student records
            fileExportProcessor.StudentRequests = NsldsContext.ClientRequestStudents
                .Where(x => x.Link2ClientRequest == id && !x.IsDeleted);

            bool success = fileExportProcessor.Process(User, id);

            var result = new FileUploadResult
            {
                BatchId = id,
                FileName = (success) ? fileExportProcessor.FileName : null,
                RouteData = (success) ? fileExportProcessor.FileRoute : null,
                IsValid = ValidateClientRequest(id),
                Message = fileExportProcessor.ErrorMessage
            };

            return Ok(result);
        }

        /// <summary>
        /// Purge and export a batch invalid students to Excel spreadsheet
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/Requests/Purge/batchid
      //  [Authorize(Policy = "Editor")]
        [HttpGet("Purge/{id}", Name = "PurgeBatchRequest")]
        public IActionResult PurgeBatchRequest([FromRoute] int id, [FromServices] FileExportProcessor fileExportProcessor)
        {
            if (ClientProfileId == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }
            if (!ClientRequestExists(id))
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidBatchId });
            }

            // load the export processor with non-valid & non-deleted student records
            fileExportProcessor.StudentRequests = NsldsContext.ClientRequestStudents
                .Where(x => x.Link2ClientRequest == id && !x.IsValid && !x.IsDeleted);

            bool success = fileExportProcessor.Process(User, id);
            bool isValid = false;
            ClientRequest cBatch = NsldsContext.ClientRequests.Single(x => x.Id == id);

            // if success exporting the invalid records to Excel, mark them deleted
            if (success)
            {
                foreach (var item in fileExportProcessor.StudentRequests)
                {
                    item.IsDeleted = true;
                    NsldsContext.Entry(item).State = EntityState.Modified;
                }
                NsldsContext.SaveChanges();

                isValid = ValidateClientRequest(id);

                // if batch is now valid, send to queue
                if (isValid)
                {
                    var job = GetOrCreateJobForRequest(cBatch);
                }
            }

            // set batch status
            SetClientRequestStatus(cBatch);

            var result = new FileUploadResult
            {
                BatchId = id,
                FileName = (success) ? fileExportProcessor.FileName : null,
                RouteData = (success) ? fileExportProcessor.FileRoute : null,
                IsValid = isValid,
                Status = cBatch.Status,
                Message = fileExportProcessor.ErrorMessage
            };

            return Ok(result);
        }

        /// <summary>
        /// Move invalid students to a new batch
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/Requests/Move/batchid
     //   [Authorize(Policy = "Editor")]
        [HttpGet("Move/{id}", Name = "MoveBatchRequest")]
        public IActionResult MoveBatchRequest([FromRoute] int id)
        {
            if (ClientProfileId == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }
            if (!ClientRequestExists(id))
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidBatchId });
            }

            bool isValid = ValidateClientRequest(id);

            if (isValid)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.BatchHasNoRecords });
            }

            // verify that batch has valid records, so as to not move an entire batch
            //var total = NsldsContext.ClientRequestStudents
            //    .Count(x => x.Link2ClientRequest == id);

            // load the invalid student records from current batch
            var students = NsldsContext.ClientRequestStudents
                .Where(x => x.Link2ClientRequest == id && !x.IsValid);

            ClientRequest cBatch = NsldsContext.ClientRequests.Single(x => x.Id == id);

            // create new client request and add invalid student records
            ClientRequest newBatch = new ClientRequest
            {
                Link2ClientProfile = ClientProfileId,
                IsDeleted = false,
                IsReceived = false,
                IsSubmitted = false,
                RevOn = DateTime.Now,
                RevBy = UserName
            };
            NsldsContext.ClientRequests.Add(newBatch);
            NsldsContext.SaveChanges();
            var batchId = newBatch.Id;

            // move invalid students to new batch
            foreach (var item in students)
            {
                item.Link2ClientRequest = batchId;
                NsldsContext.Entry(item).State = EntityState.Modified;
            }
            NsldsContext.SaveChanges();

            // revalidate current batch
            isValid = ValidateClientRequest(id);
            // if batch is now valid, send to queue
            if (isValid)
            {
                var job = GetOrCreateJobForRequest(cBatch);
            }

            // set new batch status
            SetClientRequestStatus(newBatch);

            var result = new ClientNsldsRequestResult
            {
                BatchId = batchId,
                IsValid = false,
                Status = newBatch.Status,
                RouteData = string.Format(RouteConstant.Request, batchId)
            };

            return Ok(result);
        }

        /// <summary>
        /// Process response files
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        /// POST: api/Requests/Process
       // [Authorize(Policy = "Editor")]
        [HttpPost("Process", Name = "ProcessResponseFiles")]
        public IActionResult ProcessResponseFiles(IEnumerable<IFormFile> files)
        {
            if (ClientProfileId == 0)
            {
                return BadRequest(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }

            // nslds-110: only allow manual upload methods specified in client profile
            //string[] manualUpload = { UploadMethod.TdManual, UploadMethod.EdConnect };
            //if (!manualUpload.Contains(cp.Upload_Method))
            //{
            //    return BadRequest(new ErrorResult { Message = ErrorConstant.InvalidUploadMethod });
            //}

            if (files == null || files?.Count() == 0)
            {
                return BadRequest(new ErrorResult { Message = ErrorConstant.InvalidFile });
            }

            string[] validFiles = { FileNameConstant.TRNINFOP, FileNameConstant.FAHEXTOP, FileNameConstant.TRALRTOP };

            // unique upload identifier using guid without dashes
            var uploadId = Guid.NewGuid().ToString("N");

            //var RootPath = HostingEnvironment.WebRootPath;
            var RootPath = Configuration["AppSettings:UploadRootFolder"];
            var Date = DateTime.Today.ToString("yyyy-MM-dd");
            var uploadFolder = Path.Combine(RootPath, OpeId, Date, uploadId);
            int count = 0;

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            foreach (var file in files)
            {
                var fileName = ContentDispositionHeaderValue
                    .Parse(file.ContentDisposition).FileName.Trim('"').ToUpper();
                
                // datachk-165: determine file type from headers, not filename
                //if (!validFiles.Contains(Path.GetFileNameWithoutExtension(fileName))) { continue; }
                count++;
                var newFile = Path.ChangeExtension(fileName, $"{count:D3}");
                // file.SaveAs(Path.Combine(uploadFolder, newFile));
                using (var stream = new FileStream(Path.Combine(uploadFolder, newFile), FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }

            if (count == 0)
            {
                return BadRequest(new ErrorResult { Message = ErrorConstant.InvalidFile });
            }

            var newFiles = Directory.GetFiles(uploadFolder);

            // prepare output result class
            var validRequests = NsldsContext.ClientRequests.Where(x => 
                x.Link2ClientProfile == ClientProfileId && 
                x.IsSubmitted && !x.IsReceived && !x.IsOnHold && !x.IsDeleted).ToList();

            foreach (var item in validRequests)
            {
                item.IsValid = true;
                item.RouteData = string.Format(RouteConstant.Request, item.Id);
                item.Status = NsldsContext.ClientRequestStudents
                    .Count(x => x.Link2ClientRequest == item.Id && !x.IsDeleted).ToString();
            }

            var Result = new ResponseFilesResult
            {
                UploadId = uploadId,
                UploadDate = Date,
                FileCount = newFiles.Count()
            };

            // analyse uploaded files
            var resp = new ResponseProcessor();
            resp.GlobalContext = GlobalContext;
            resp.NsldsContext = NsldsContext;

            foreach (var file in newFiles)
            {
                var batches = resp.Analyse(OpeId, file);
                if (resp.HasErrors)
                {
                    return BadRequest(new ErrorResult { Message = ErrorConstant.InvalidFileUpload });
                }

                foreach (var batch in batches)
                {
                    // check if an identical batch already exists in results
                    var _batch = Result.Results
                        .FirstOrDefault(x => x.SubmittedOn == batch.SubmittedOn && x.Sequence == batch.Sequence);
                    // TRALRTOP responses are standalone new batch requests
                    if (_batch != null && !_batch.FileName.Contains(FileNameConstant.TRALRTOP) && batch.FileName != FileNameConstant.TRALRTOP)
                    {
                        _batch.FileName = string.Join("/", _batch.FileName, batch.FileName);
                        continue;
                    }
                    batch.UploadId = uploadId;
                    batch.UploadDate = Date;
                    batch.StartDate = DateTime.Today;
                    // try to find a matching request except for TRALRTOP responses
                    batch.BatchId = (batch.FileName == FileNameConstant.TRALRTOP) ? 0 : validRequests
                        .FirstOrDefault(x => 
                        x.SubmittedOn.Value.Date == batch.SubmittedOn && x.Sequence == batch.Sequence)
                        ?.Id ?? 0;
                    if (batch.BatchId > 0)
                    {
                        var request = validRequests.Single(x => x.Id == batch.BatchId);
                        Result.ValidRequests.Add(request);
                        validRequests.Remove(request);
                    }

                    Result.Results.Add(batch);
                }
            }

            return Ok(Result);
        }

        /// <summary>
        /// Confirm batch identification match to response batch
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        /// PUT: api/Requests/Process
       // [Authorize(Policy = "Editor")]
        [HttpPut("Process", Name = "ProcessResponseBatch")]
        public IActionResult ProcessResponseBatch([FromServices] Quartz.IScheduler Scheduler, [FromBody] ResponseBatchResult batch)
        {
            if (ClientProfileId == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }

            if (batch.Ignore) { return Ok(); }

            // user selected existing batch request
            if (batch.BatchId > 0 && !ClientRequestExists(batch.BatchId))
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidBatchId });
            }

            var RootPath = Configuration["AppSettings:UploadRootFolder"];
            var resp = new ResponseProcessor();
            resp.GlobalContext = GlobalContext;
            resp.NsldsContext = NsldsContext;
            var result = resp.Process(batch, RootPath, OpeId, UserName);

            if (resp.HasErrors)
            {
                return BadRequest(new ErrorResult { Message = result });
            }

            // configure our scheduler service access
            var jobdata = new Quartz.JobDataMap();
            jobdata.Add("opeid", OpeId);
            jobdata.Add("user", UserName);
            jobdata.Add("jobdate", DateTime.Today);
            jobdata.Add("batch", batch);

            // define the jobs and tie it to our ResponseJobProcessor classes
            var job = Quartz.JobBuilder.Create<ResponseJobProcessor>()
                .WithIdentity($"BatchId_{batch.BatchId}", OpeId)
                .UsingJobData(jobdata)
                .Build();

            var jobdelay = Configuration.GetValue<int>("AppSettings:QueueJobDelay");

            var trigger = Quartz.TriggerBuilder.Create()
                //.WithIdentity($"BatchId_{batch.BatchId}", "process")
                .StartAt(DateTime.UtcNow.AddSeconds(jobdelay)) // delay defined in appsettings
                .Build();
            // place job in queue
            Scheduler.ScheduleJob(job, trigger);

            // return processed request
            var resultJson = new
            {
                BatchId = batch.BatchId,
                Message = result,
                RouteData = string.Format(RouteConstant.Request, batch.BatchId)
            };

            return Ok(resultJson);
        }

        /// <summary>
        /// retrieve the status of a batch being processed
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/Requests/Status/4
      //  [Authorize(Policy = "Editor")]
        [HttpGet("Status/{id}", Name = "GetBatchStatusById")]
        public IActionResult GetBatchStatusById([FromRoute] int id)
        {
            if (ClientProfileId == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }

            if (!ClientRequestExists(id))
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidBatchId });
            }

            var batch = NsldsContext.ClientRequests.Single(x => x.Id == id);

            SetClientRequestStatus(batch);

            var resultJson = new
            {
                BatchId = batch.Id,
                Status = batch.Status,
                RouteData = string.Format(RouteConstant.Request, batch.Id)
            };

            return Ok(resultJson);
        }

        /// <summary>
        /// Generate the nslds batch EdConnect inform file by batchId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/Requests/Submit/4
      //  [Authorize(Policy = "Editor")]
        [HttpGet("Submit/{id}", Name = "SubmitNsldsBatchById")]
        public IActionResult SubmitNsldsBatchById([FromRoute] int id)
        {
            if (ClientProfileId == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }

            // nslds-110: only allow manual upload methods specified in client profile
            string[] manualUpload = { UploadMethod.TdManual, UploadMethod.EdConnect };
            if (!manualUpload.Contains(cp.Upload_Method))
            {
                return BadRequest(new ErrorResult { Message = ErrorConstant.InvalidUploadMethod });
            }

            if (ClientRequestExists(id))
            {
                // test validation of the current batch to make sure it is valid
                var currentBatch = NsldsContext.ClientRequests.Single(x => x.Id == id);
                var isValid = ValidateClientRequest(id);

                // check that this batch is valid and not already submitted
                if (!isValid)
                {
                    return BadRequest(new ErrorResult { Message = ErrorConstant.InvalidBatch });
                }
                else if (currentBatch.ReceivedOn.HasValue)
                {
                    return BadRequest(new ErrorResult { Message = ErrorConstant.BatchAlreadyReceived });
                }
                else if (currentBatch.IsDeleted || currentBatch.IsOnHold)
                {
                    return BadRequest(new ErrorResult { Message = ErrorConstant.BatchOnHoldOrDisabled });
                }

                var msg = string.Empty;

                if (currentBatch.IsSubmitted)
                {
                    var informURL = currentBatch.Response;
                    string[] fileParts = informURL.Split('/');
                    var informFile = Path.Combine(HostingEnvironment.WebRootPath, Path.Combine(fileParts));

                    if (!System.IO.File.Exists(informFile))
                    {
                        msg = MessageConstant.InformFileNotFound;
                    }
                    else
                    {
                        msg = MessageConstant.InformFileRetrieved;
                    }
                }
                else try
                    {
                        // generate output to disk file in webroot folder
                        msg = MessageConstant.InformFileGenerated;
                        GenerateOutputFile(id, (cp.Upload_Method == UploadMethod.EdConnect) ? false : true);
                    }
                    catch (Exception ex)
                    {
                        msg = ex.Message;
                    }

                var cBatch = NsldsContext.ClientRequests
                    .Single(x => x.Id == id);

                cBatch.IsValid = true;
                SetClientRequestStatus(cBatch);

                return Ok(new InformFileResult
                {
                    BatchId = cBatch.Id,
                    Sequence = cBatch.Sequence,
                    Status = cBatch.Status,
                    RouteData = cBatch.Response,
                    Message = msg
                });
            }
            else
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidBatchId });
            }
        }

        #endregion

        #region Test endpoints

        /// <summary>
        /// Form for uploading. TEST ONLY
        /// </summary>
        /// <returns></returns>
        // test upload form
        //[Authorize(Policy = "Administrator")]
        //[HttpGet("Upload")]
        //public IActionResult Upload()
        //{
        //    return View();
        //}

        /// <summary>
        /// View token content (Test)
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        // GET: api/Requests/Token
        [HttpGet("Token", Name = "ViewToken")]
        public IActionResult ViewToken()
        {
            var token = User.Claims.ToList();
            var claims = new List<string>();
            foreach (var item in token)
            {
                claims.Add($"{item.Type} : {item.Value}");
            }
            return Ok(claims);
        }

		//  TEST

		public class Person: Controller
		{
			string fName;
			string lName;

			
			public Person(string first, string last)
			{
				fName = first;
				lName = last;
			}

			[HttpGet("{fName}/{lName}")]
			public Person GetPerson(string firstName, string lastName)
			{
				Person p = new Person(firstName, lastName);
				return p;
			}


		}

			// END TEST

        /// <summary>
        /// Simulate an exception (Test)
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        // GET: api/Requests/Throw
      //  [Authorize(Policy = "Administrator")]
        [HttpGet("Throw", Name = "ThrowException")]
        public IActionResult ThrowException()
        {
            throw new Exception("Simulated exception sent to Sentry from NSLDS.API");
        }

        //[Authorize(Policy = "Administrator")]
        //[HttpGet("query", Name = "TestQuery")]
        //public IActionResult TestQuery()
        //{
        //    var tmStudents = (from s in NsldsContext.ClientRequestStudents
        //                     join r in NsldsContext.ClientRequests on s.Link2ClientRequest equals r.Id
        //                     where (r.Link2ClientProfile == cp.Id && r.IsTM == true) &&
        //                     r.ReceivedOn.HasValue &&
        //                     FahDef.ValidUntil(r.ReceivedOn.Value, cp.Expiration) < DateTime.Today &&
        //                     !s.IsRefreshed && !s.IsDeleted &&
        //                     (s.IsLoanReviewed == false || s.IsPellReviewed == false ||
        //                      s.IsGrantReviewed == false || s.IsTeachReviewed == false)
        //                     select s).ToList();

        //    StringBuilder sb = null;

        //    if (tmStudents.Count() > 0)
        //    {
        //        // check for existing job queue available, get last job not submitted or null
        //        var job = NsldsContext.Jobs.LastOrDefault(x =>
        //          x.Link2ClientProfile == cp.Id &&
        //          x.JobDate == DateTime.Today);

        //        // if job is null create a new one
        //        if (job == null)
        //        {
        //            job = new Job
        //            {
        //                Link2ClientProfile = cp.Id,
        //                JobDate = DateTime.Today,
        //                SubmittedOn = DateTime.Now,
        //                RevOn = DateTime.Now,
        //                RevBy = "TdClient Scheduler"
        //            };
        //            NsldsContext.Jobs.Add(job);
        //            NsldsContext.SaveChanges();
        //        }
        //        var newBatch = new ClientRequest
        //        {
        //            Link2ClientProfile = cp.Id,
        //            RevBy = "TdClient Scheduler",
        //            RevOn = DateTime.Now,
        //            SubmittedOn = DateTime.Now,
        //            Response = StatusConstant.TmRefresh,
        //            Sequence = 10,
        //            Link2Job = job.Id
        //        };
        //        NsldsContext.ClientRequests.Add(newBatch);
        //        NsldsContext.SaveChanges();
        //        try
        //        {
        //            // same query without duplicates for TM refresh
        //            var newStudents = tmStudents.GroupBy(x => x.SSN).Select(s => s.First()).ToList();
                    
        //            // initialize automapper to clone student requests
        //            Mapper.Initialize(cfg =>
        //            {
        //                cfg.CreateMap<ClientRequestStudent, ClientRequestStudent>()
        //                .ForMember(dest => dest.Id, opt => opt.Ignore())
        //                .ForMember(dest => dest.Link2ClientRequest, opt => opt.UseValue(newBatch.Id));
        //            });

        //            foreach (var student in newStudents)
        //            {
        //                // Clone student record and add to new batch request
        //                var newStudent = Mapper.Map<ClientRequestStudent, ClientRequestStudent>(student);
        //                newStudent.With(s =>
        //                {
        //                    s.IsSubmitted = true;
        //                    s.SubmittedOn = DateTime.Now;
        //                    s.IsReceived = false;
        //                    s.ReceivedOn = null;
        //                    s.Response = null;
        //                    s.IsPellReviewed = null;
        //                    s.IsLoanReviewed = null;
        //                    s.IsGrantReviewed = null;
        //                    s.IsTeachReviewed = null;
        //                });
        //                NsldsContext.ClientRequestStudents.Add(newStudent);                    
        //            }

        //            // prepare the batch inform request
        //            sb = BatchInformBuilder.Build(cp, newBatch);

        //            foreach (var student in tmStudents)
        //            {
        //                student.IsRefreshed = true;
        //                NsldsContext.Entry(student).State = EntityState.Modified;
        //            }

        //            newBatch.IsSubmitted = true;
        //            NsldsContext.Entry(newBatch).State = EntityState.Modified;
        //            NsldsContext.SaveChanges();
        //        }
        //        catch (Exception)
        //        {
        //            newBatch.Students = null;
        //            newBatch.IsFailed = true;
        //            NsldsContext.Entry(newBatch).State = EntityState.Modified;
        //            NsldsContext.SaveChanges();
        //        }
        //    }

        //    return Ok(sb.ToString());
        //}

        #endregion
    }
}