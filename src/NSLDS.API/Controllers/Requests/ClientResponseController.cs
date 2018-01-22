using System;
using System.Collections.Generic;
using System.Linq;
using Global.Domain;
using NSLDS.Domain;
using Microsoft.Extensions.Logging;
using NSLDS.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace NSLDS.API.Controllers
{
   // [Authorize]
    [Produces("application/json")]
    [Route("api/Response")]
    public class ClientResponseController : DbContextController
    {
        #region Private Methods & fields

        // fah fields and codes only loaded when needed and once per session
        private List<FahAlert> _fa;
        private List<FahField> _ff;
        private List<FahCode> _fc;
        private List<PellAward> _fp;

        private List<FahAlert> fa
        {
            get { if (_fa == null) _fa = GlobalContext.FahAlerts.ToList(); return _fa; }
        }

        private List<FahField> ff
        {
            get { if (_ff == null) _ff = GlobalContext.FahFields.ToList(); return _ff; }
        }

        private List<FahCode> fc
        {
            get { if (_fc == null) _fc = GlobalContext.FahCodes.ToList(); return _fc; }
        }

        private List<PellAward> fp
        {
            get { if (_fp == null) _fp = GlobalContext.PellAwards.ToList(); return _fp; }
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
            int count = NsldsContext.ClientRequestStudents
                .Count(x => x.Link2ClientRequest == id && !x.IsDeleted && !x.IsValid);
            return (count == 0) ? true : false;
        }

        private void SetClientRequestStatus(ClientRequest cBatch)
        {
            if (cBatch.IsDeleted)
            {
                cBatch.Status = StatusConstant.Disabled;
            }
            else if (cBatch.IsReceived)
            {
                cBatch.Status = StatusConstant.Received;
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
            else if (cReq.IsReceived)
            {
                cReq.Status = StatusConstant.Received;
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

        private bool ClientResponseNsldsExists(int id)
        {
            return NsldsContext.ClientRequestStudents.Any(e => e.Id == id && e.IsReceived);
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

        public ClientResponseController(GlobalContext globalContext, IHostingEnvironment hostingEnvironment, IConfiguration configuration, IRuntimeOptions runtimeOptions, ILogger<ClientResponseController> logger)
            : base(globalContext, hostingEnvironment, configuration, runtimeOptions, logger)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get nslds FAH summary record by studentId
        /// </summary>
        /// <param name="id"></param>
        /// <param name="resolve"></param>
        /// <returns></returns>
        // GET: api/Response/4
      //  [Authorize(Policy = "FileReview")]
        [HttpGet("{id}", Name = "GetNsldsResponseById")]
        public IActionResult GetNsldsResponseById([FromRoute] int id, [FromQuery] bool resolve = false)
        {
            if (ClientProfileId == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }
            if (ClientResponseNsldsExists(id))
            {
                // retrieve client profile
                var client = NsldsContext.ClientProfiles.Single(x => x.Id == ClientProfileId);

                // retrieve student and type1 Fah record, if there is one
                var student = NsldsContext.ClientRequestStudents.Single(x => x.Id == id);
                var fahType1 = NsldsContext.FahType1Recs
                    .FirstOrDefault(x => x.Link2ClientRequestStudent == id);

                // no FAH data available
                if (fahType1 == null)
                {
                    return NotFound();
                }

                // retrieve type 2 name history record and check for name change
                var fahType2 = NsldsContext.FahType2Recs
                    .FirstOrDefault(x =>
                    x.Link2ClientRequestStudent == id &&
                    (x.CurrentFirstName != x.FirstNameHistory ||
                    x.CurrentLastName != x.LastNameHistory));

                // retrieve type 5 loan codes that generate exceptions
                var fahType5 = NsldsContext.FahType5Recs
                    .Where(x => x.Link2ClientRequestStudent == id).ToList();

                // filter type 5 loan types for open AY calc
                var fahType5AY = fahType5.Where(x =>
                    FahDef.loanTypes.Contains(x.LoanTypeCode) && x.LoanStatusCode != FahDef.CAStatus).ToList();

                // retrieve type 4 grant types for award year calc
                var fahType4 = NsldsContext.FahType4Recs
                    .Where(x => x.Link2ClientRequestStudent == id).ToList();

                // nslds-137: revert to using type1 leu due to possible duplicates records from nslds
                //decimal granttotal = fahType4
                //    .Where(x => x.GrantType == FahDef.grantPE).Sum(x => x.EligibilityUsed) ?? 0;
                decimal type1leu = fahType1.LifeEligibilityUsed ?? 0;
                if (type1leu > 0) { type1leu += 0.0009M; }
                decimal percentused = type1leu; //Math.Max(granttotal, type1leu);

                // retrieve type 3 grant flags for LEU remaining & award year calc
                var fahType3 = NsldsContext.FahType3Recs
                    .Where(x => x.Link2ClientRequestStudent == id).ToList();

                bool pellflag = FahDef.HasPellFlag(fahType3);
                bool grantflag = fahType3.Any(x => FahDef.grantFlags.Contains(x.Overpayment));

                // check death or default loan status and student.isresolved flag
                var loanflag = FahDef.DeathOrDefault(fahType5, fahType1, grantflag);
                if (loanflag == null && !student.IsResolved)
                {
                    student.IsResolved = true;
                    NsldsContext.Entry(student).State = EntityState.Modified;
                    NsldsContext.SaveChanges();
                }

                // NSLDS-141
                // check resolved flag
                var resolved = (resolve || loanflag == null);

                var result = new NsldsSummaryResult();

                // add all summary fields & values
                result.NameHistory.Display = result.AlertName(x => x.NameHistory, fa);
                result.NameHistory.Description = result.AlertDescription(x => x.NameHistory, fa);
                result.NameHistory.Changed = fahType2 != null;
                if (result.NameHistory.Changed)
                {
                    result.NameHistory.FirstName.Display =
                        result.NameHistory.AlertName(x => x.FirstName, fa);
                    result.NameHistory.FirstName.Value = fahType2?.FirstNameHistory;
                    result.NameHistory.LastName.Display =
                        result.NameHistory.AlertName(x => x.LastName, fa);
                    result.NameHistory.LastName.Value = fahType2?.LastNameHistory;
                }

                result.ValidUntil.Display = result.AlertName(x => x.ValidUntil, fa);
                result.ValidUntil.Description = result.AlertDescription(x => x.ValidUntil, fa);
                result.ValidUntil.Value = FahDef.IsValidUntil(student.ReceivedOn, client.Expiration);

                result.UnusualEnrollment.Display = result.AlertName(x => x.UnusualEnrollment, fa);
                result.UnusualEnrollment.Description = fahType1.UnusualEnrollHistory.CodeDescription(FahDef.UnusualEnrollment, fc);
                result.UnusualEnrollment.Value = FahDef.CodeOrNA(fahType1.UnusualEnrollHistory);

                // Alerts
                result.Alerts.Display = result.AlertName(x => x.Alerts, fa);
                result.Alerts.Description = result.AlertDescription(x => x.Alerts, fa);
                result.Alerts.Value = FahDef.Exceptions(fahType1, fahType5, grantflag);

                // Resolved flag used when certain alerts are present
                result.IsResolved = resolved;

                // undergraduate loan aggregate limit calculation engine
                var lnagg = new LoanAggregateLimitCalc(fahType1, resolved);

                result.UndergraduateLoans.Display = result.AlertName(x => x.UndergraduateLoans, fa);
                result.UndergraduateLoans.Description = result.AlertDescription(x => x.UndergraduateLoans, fa);

                result.UndergraduateLoans.Subsidized.Display =
                    result.UndergraduateLoans.AlertName(x => x.Subsidized, fa);
                result.UndergraduateLoans.Subsidized.Description =
                    result.UndergraduateLoans.AlertDescription(x => x.Subsidized, fa);

                result.UndergraduateLoans.Subsidized.Dependent.Display =
                    result.UndergraduateLoans.Subsidized.AlertName(x => x.Dependent, fa);
                result.UndergraduateLoans.Subsidized.Dependent.Description =
                    result.UndergraduateLoans.Subsidized.AlertDescription(x => x.Dependent, fa);
                result.UndergraduateLoans.Subsidized.Dependent.Value = (!resolved) ? loanflag : lnagg.SubDependent;

                result.UndergraduateLoans.Subsidized.Independent.Display =
                    result.UndergraduateLoans.Subsidized.AlertName(x => x.Independent, fa);
                result.UndergraduateLoans.Subsidized.Independent.Description =
                    result.UndergraduateLoans.Subsidized.AlertDescription(x => x.Independent, fa);
                result.UndergraduateLoans.Subsidized.Independent.Value = (!resolved) ? loanflag : lnagg.SubIndependent;

                result.UndergraduateLoans.Subsidized.OutstandingPrincipalBal.Display =
                    result.UndergraduateLoans.Subsidized.AlertName(x => x.OutstandingPrincipalBal, fa);
                result.UndergraduateLoans.Subsidized.OutstandingPrincipalBal.Description =
                    result.UndergraduateLoans.Subsidized.AlertDescription(x => x.OutstandingPrincipalBal, fa);
                result.UndergraduateLoans.Subsidized.OutstandingPrincipalBal.Value = $"{fahType1.UndergradSubPrincipalBal ?? 0:C}";

                result.UndergraduateLoans.Subsidized.PendingDisbursement.Display =
                    result.UndergraduateLoans.Subsidized.AlertName(x => x.PendingDisbursement, fa);
                result.UndergraduateLoans.Subsidized.PendingDisbursement.Description =
                    result.UndergraduateLoans.Subsidized.AlertDescription(x => x.PendingDisbursement, fa);
                result.UndergraduateLoans.Subsidized.PendingDisbursement.Value = $"{fahType1.UndergradSubPendingDisb ?? 0:C}";

                result.UndergraduateLoans.Unsubsidized.Display =
                    result.UndergraduateLoans.AlertName(x => x.Unsubsidized, fa);
                result.UndergraduateLoans.Unsubsidized.Description =
                    result.UndergraduateLoans.AlertDescription(x => x.Unsubsidized, fa);

                result.UndergraduateLoans.Unsubsidized.Dependent.Display =
                    result.UndergraduateLoans.Unsubsidized.AlertName(x => x.Dependent, fa);
                result.UndergraduateLoans.Unsubsidized.Dependent.Description =
                    result.UndergraduateLoans.Unsubsidized.AlertDescription(x => x.Dependent, fa);
                result.UndergraduateLoans.Unsubsidized.Dependent.Value = (!resolved) ? loanflag : lnagg.UnsubDependent;

                result.UndergraduateLoans.Unsubsidized.Independent.Display =
                    result.UndergraduateLoans.Unsubsidized.AlertName(x => x.Independent, fa);
                result.UndergraduateLoans.Unsubsidized.Independent.Description =
                    result.UndergraduateLoans.Unsubsidized.AlertDescription(x => x.Independent, fa);
                result.UndergraduateLoans.Unsubsidized.Independent.Value = (!resolved) ? loanflag : lnagg.UnsubIndependent;

                result.UndergraduateLoans.Unsubsidized.OutstandingPrincipalBal.Display =
                    result.UndergraduateLoans.Unsubsidized.AlertName(x => x.OutstandingPrincipalBal, fa);
                result.UndergraduateLoans.Unsubsidized.OutstandingPrincipalBal.Description =
                    result.UndergraduateLoans.Unsubsidized.AlertDescription(x => x.OutstandingPrincipalBal, fa);
                result.UndergraduateLoans.Unsubsidized.OutstandingPrincipalBal.Value = $"{fahType1.UndergradUnsubPrincipalBal ?? 0:C}";

                result.UndergraduateLoans.Unsubsidized.PendingDisbursement.Display =
                    result.UndergraduateLoans.Unsubsidized.AlertName(x => x.PendingDisbursement, fa);
                result.UndergraduateLoans.Unsubsidized.PendingDisbursement.Description =
                    result.UndergraduateLoans.Unsubsidized.AlertDescription(x => x.PendingDisbursement, fa);
                result.UndergraduateLoans.Unsubsidized.PendingDisbursement.Value = $"{fahType1.UndergradUnsubPendingDisb ?? 0:C}";

                // Graduate limits
                result.GraduateLoans.Display = result.AlertName(x => x.GraduateLoans, fa);
                result.GraduateLoans.Description = result.AlertDescription(x => x.GraduateLoans, fa);
                // Subsidized
                result.GraduateLoans.Subsidized.Display =
                    result.GraduateLoans.AlertName(x => x.Subsidized, fa);
                result.GraduateLoans.Subsidized.Description =
                    result.GraduateLoans.AlertDescription(x => x.Subsidized, fa);
                result.GraduateLoans.Subsidized.Value = $"{0:C}";

                result.GraduateLoans.SubOutstandingPrincipalBal.Display =
                    result.GraduateLoans.AlertName(x => x.OutstandingPrincipalBal, fa);
                result.GraduateLoans.SubOutstandingPrincipalBal.Description =
                    result.GraduateLoans.AlertDescription(x => x.OutstandingPrincipalBal, fa);
                result.GraduateLoans.SubOutstandingPrincipalBal.Value = $"{fahType1.GradSubPrincipalBal ?? 0:C}";

                result.GraduateLoans.SubPendingDisbursement.Display =
                    result.GraduateLoans.AlertName(x => x.PendingDisbursement, fa);
                result.GraduateLoans.SubPendingDisbursement.Description =
                    result.GraduateLoans.AlertDescription(x => x.PendingDisbursement, fa);
                result.GraduateLoans.SubPendingDisbursement.Value = $"{fahType1.GradSubPendingDisb ?? 0:C}";
                //Unsubsidized
                result.GraduateLoans.Unsubsidized.Display =
                    result.GraduateLoans.AlertName(x => x.Unsubsidized, fa);
                result.GraduateLoans.Unsubsidized.Description =
                    result.GraduateLoans.AlertDescription(x => x.Unsubsidized, fa);
                result.GraduateLoans.Unsubsidized.Value = (!resolved) ? loanflag : lnagg.GradTotal;

                result.GraduateLoans.OutstandingPrincipalBal.Display =
                    result.GraduateLoans.AlertName(x => x.OutstandingPrincipalBal, fa);
                result.GraduateLoans.OutstandingPrincipalBal.Description =
                    result.GraduateLoans.AlertDescription(x => x.OutstandingPrincipalBal, fa);
                result.GraduateLoans.OutstandingPrincipalBal.Value = $"{fahType1.GradUnsubPrincipalBal ?? 0:C}";

                result.GraduateLoans.PendingDisbursement.Display =
                    result.GraduateLoans.AlertName(x => x.PendingDisbursement, fa);
                result.GraduateLoans.PendingDisbursement.Description =
                    result.GraduateLoans.AlertDescription(x => x.PendingDisbursement, fa);
                result.GraduateLoans.PendingDisbursement.Value = $"{fahType1.GradUnsubPendingDisb ?? 0:C}";

                // open AY loans
                result.OpenAY.Display = result.AlertName(x => x.OpenAY, fa);
                result.OpenAY.Description = result.AlertDescription(x => x.OpenAY, fa);
                result.OpenAY.Value = FahDef.CalcOpenAY(student.StartDate, fahType5AY, client.AY_Definition);

                result.SULA.Display = result.AlertName(x => x.SULA, fa);
                result.SULA.Description = result.AlertDescription(x => x.SULA, fa);

                result.SULA.Flag.Display = result.SULA.AlertName(x => x.Flag, fa);
                result.SULA.Flag.Description = result.SULA.AlertDescription(x => x.Flag, fa);
                result.SULA.Flag.Value = FahDef.YOrN(fahType1.SulaFlag);

                result.SULA.Usage.Display = result.SULA.AlertName(x => x.Usage, fa);
                result.SULA.Usage.Description = result.SULA.AlertDescription(x => x.Usage, fa);
                result.SULA.Usage.Value = FahDef.CalcSulaUsage(fahType1.SubUsagePeriod);

                result.Grants.Display = result.AlertName(x => x.Grants, fa);
                result.Grants.Description = result.AlertDescription(x => x.Grants, fa);

                result.Grants.PellLEU.Display = result.Grants.AlertName(x => x.PellLEU, fa);
                result.Grants.PellLEU.Description = result.Grants.AlertDescription(x => x.PellLEU, fa);
                result.Grants.PellLEU.Value = FahDef.CalcLeuRemain(percentused, resolved, loanflag);

                result.Grants.AnnualAwards.Display = result.Grants.AlertName(x => x.AnnualAwards, fa);
                result.Grants.AnnualAwards.Description = result.Grants.AlertDescription(x => x.AnnualAwards, fa);
                result.Grants.AnnualAwards.Values = FahDef.CalcAnnualAwards(student, fahType1, fahType4, pellflag, resolved, fp, null, null, null, null, null, null, null, null);

                result.OtherFunding.Display = result.AlertName(x => x.OtherFunding, fa);
                result.OtherFunding.Description = FahDef.CalcOtherFunding(fahType1, fahType4, fc);
                result.OtherFunding.Value = (result.OtherFunding.Description != null) ? FahDef.Y : null;

                return Ok(result);
            }
            else
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.NsldsRequestNotFound });
            }
        }

        /// <summary>
        /// Resolve/unresolve a response FAH or TM alert or change startdate by studentId
        /// </summary>
        /// <param name="id"></param>
        /// <param name="resolve"></param>
        /// <param name="startdate"></param>
        /// <param name="loanreviewed"></param>
        /// <param name="pellreviewed"></param>
        /// <param name="grantreviewed"></param>
        /// <param name="teachreviewed"></param>
        /// <returns></returns>
      //  [Authorize(Policy = "FileReview")]
        [HttpPut("{id}", Name = "ResolveAlertById")]
        public IActionResult ResolveAlertById([FromRoute] int id, [FromQuery] bool? resolve, [FromQuery] DateTime? startdate, [FromQuery] bool? loanreviewed, [FromQuery] bool? pellreviewed, [FromQuery] bool? grantreviewed, [FromQuery] bool? teachreviewed)
        {
            if (ClientProfileId == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }
            if (!ClientResponseNsldsExists(id))
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.NsldsRequestNotFound });
            }
            // retrieve student record & update parameter values
            var student = NsldsContext.ClientRequestStudents.Single(x => x.Id == id);

            // NSLDS-141: don't save the resolve flag anymore
            //if (resolve.HasValue) { student.IsResolved = resolve.Value; }
            if (startdate.HasValue) { student.StartDate = startdate.Value; }
            // datachk-68: TM alert review flags
            if (loanreviewed.HasValue) { student.IsLoanReviewed = loanreviewed.Value; }
            if (pellreviewed.HasValue) { student.IsPellReviewed = pellreviewed.Value; }
            if (grantreviewed.HasValue) { student.IsGrantReviewed = grantreviewed.Value; }
            if (teachreviewed.HasValue) { student.IsTeachReviewed = teachreviewed.Value; }
            // mark student refreshed if all alerts are reviewed
            student.IsRefreshed =
                (!student.IsGrantReviewed.HasValue || student.IsGrantReviewed.Value) &&
                (!student.IsLoanReviewed.HasValue || student.IsLoanReviewed.Value) &&
                (!student.IsPellReviewed.HasValue || student.IsPellReviewed.Value) &&
                (!student.IsTeachReviewed.HasValue || student.IsTeachReviewed.Value);

            NsldsContext.Entry(student).State = EntityState.Modified;
            NsldsContext.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Recalculate a response Pell eligibility by studentId
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ayrecalc"></param>
        /// <param name="resolve"></param>
        /// <returns></returns>
        // POST: api/Response/Recalc/Id
      //  [Authorize(Policy = "FileReview")]
        [HttpPost("Recalc/{id}", Name = "GetNsldsResponseRecalcById")]
        public IActionResult GetNsldsResponseRecalcById([FromRoute] int id, [FromBody] List<AwardYear> ayrecalc, [FromQuery] bool resolve = false)
        {
            if (ClientProfileId == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }
            if (!ClientResponseNsldsExists(id))
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.NsldsRequestNotFound });
            }
            
            // retrieve student record & update parameter values
            var request = NsldsContext.ClientRequestStudents.Single(x => x.Id == id);

            var fahType1 = NsldsContext.FahType1Recs
                .FirstOrDefault(x => x.Link2ClientRequestStudent == id);
            
            // no FAH data available
            if (fahType1 == null)
            {
                return NotFound();
            }

            // retrieve type 4 grant types for award year recalc
            var grants = NsldsContext.FahType4Recs
                .Where(x => x.Link2ClientRequestStudent == id).ToList();

            // retrieve type 3 grant flags for LEU remaining & award year calc
            var fahType3 = NsldsContext.FahType3Recs
                .Where(x => x.Link2ClientRequestStudent == id).ToList();

            // retrieve type 5 loan codes that generate exceptions
            var fahType5 = NsldsContext.FahType5Recs
                .Where(x => x.Link2ClientRequestStudent == id).ToList();

            bool pellflag = FahDef.HasPellFlag(fahType3);
            bool grantflag = fahType3.Any(x => FahDef.grantFlags.Contains(x.Overpayment));

            // check death or default loan status flag
            var loanflag = FahDef.DeathOrDefault(fahType5, fahType1, grantflag);
            var resolved = resolve || string.IsNullOrEmpty(loanflag);

            // datachk-187: retrieve tentative & revised amounts from ayrecalc body parameter
            decimal? revised1 = null, tentative1 = null, revised1add = null, tentative1add = null,
                revised2 = null, tentative2 = null, revised2add = null, tentative2add = null;
            bool apply1 = false, apply2 = false;

            if (ayrecalc?.Count == 2)
            {
                var ay1 = ayrecalc[0];
                var ay2 = ayrecalc[1];
                revised1 = ay1.RevisedAmount;
                tentative1 = ay1.TentativeAmount;
                apply1 = ay1.AdditionalEligibility?.Apply ?? false;
                revised1add = ay1.AdditionalEligibility?.RevisedAmount;
                tentative1add = ay1.AdditionalEligibility?.TentativeAmount;
                revised2 = ay2.RevisedAmount;
                tentative2 = ay2.TentativeAmount;
                apply2 = ay2.AdditionalEligibility?.Apply ?? false;
                revised2add = ay2.AdditionalEligibility?.RevisedAmount;
                tentative2add = ay2.AdditionalEligibility?.TentativeAmount;
            }

            // proceed with recalculation
            AwardsRecalcResult result;

            result.Values = FahDef.CalcAnnualAwards(request, fahType1, grants, pellflag, resolved, fp, revised1, revised2, tentative1, tentative2, revised1add, revised2add, tentative1add, tentative2add, apply1, apply2);

            return Ok(result);
        }

        /// <summary>
        /// Get nslds FAH detail record by studentId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/Response/4/Detail
      //  [Authorize(Policy = "FileReview")]
        [HttpGet("{id}/Detail", Name = "GetNsldsResponseDetailById")]
        public IActionResult GetNsldsResponseDetailById([FromRoute] int id, [FromQuery] bool resolve = false)
        {
            if (ClientProfileId == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }
            if (ClientResponseNsldsExists(id))
            {
                // retrieve client profile
                var client = NsldsContext.ClientProfiles.Single(x => x.Id == ClientProfileId);

                // retrieve student and type1 Fah record, if there is one
                var student = NsldsContext.ClientRequestStudents.Single(x => x.Id == id);
                var fahType1 = NsldsContext.FahType1Recs
                    .FirstOrDefault(x => x.Link2ClientRequestStudent == id);

                // no FAH data available
                if (fahType1 == null)
                {
                    return NotFound();
                }

                NsldsDetailResult result = new NsldsDetailResult();

                // retrieve type 2 name history records
                var fahType2 = NsldsContext.FahType2Recs
                    .Where(x => x.Link2ClientRequestStudent == id).ToList();
                result.NameHistory = FahDef.GetNameHistoryDetail(fahType2, fa, ff);

                // retrieve type 4 grant types for award year calc & grants
                var fahType4 = NsldsContext.FahType4Recs
                    .Where(x => x.Link2ClientRequestStudent == id).ToList();

                // retrieve type 3 grant flags for LEU remaining & award year calc
                var fahType3 = NsldsContext.FahType3Recs
                    .Where(x => x.Link2ClientRequestStudent == id).ToList();

                // retrieve type 5 loan history records
                var fahType5 = NsldsContext.FahType5Recs.Where(x => 
                    x.Link2ClientRequestStudent == id).ToList();

                bool pellflag = FahDef.HasPellFlag(fahType3);
                bool grantflag = fahType3.Any(x => FahDef.grantFlags.Contains(x.Overpayment));

                // check death or default loan status and student.isresolved flag
                var loanflag = FahDef.DeathOrDefault(fahType5, fahType1, grantflag);
                if (loanflag == null && !student.IsResolved)
                {
                    student.IsResolved = true;
                    NsldsContext.Entry(student).State = EntityState.Modified;
                    NsldsContext.SaveChanges();
                }
                // NSLDS-141: check resolved flag
                var resolved = (resolve || loanflag == null);

                result.LoanAggregateLimits = FahDef.GetLoanAggregateLimits(fahType1, fahType5, fa, fc, resolved, loanflag);

                // loans used for open AY aggregate limits
                var ayType5 = fahType5.Where(x =>
                    FahDef.loanTypes.Contains(x.LoanTypeCode) && x.LoanStatusCode != FahDef.CAStatus).ToList();

                result.OpenAcademicYear = 
                    FahDef.GetOpenAcademicYear(student.StartDate, client.AY_Definition, fahType1, ayType5, fa, fc, resolved, loanflag);

                result.SULA = FahDef.GetSubUsageLimit(fahType1, fa);

                // other funding
                result.OtherFunding = FahDef.HasOtherFunding(fahType1, fahType4);

                result.Grants = FahDef.GetGrants(student, fahType1, fahType4, fa, fp, pellflag, resolved, loanflag);

                result.Loans = FahDef.GetLoans(fahType1, fahType5, fa, fc);

                return Ok(result);
            }
            else
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.NsldsRequestNotFound });
            }
        }

        /// <summary>
        /// Export batch or student loan history to Excel spreadsheet
        /// </summary>
        /// <param name="batchid"></param>
        /// <param name="studentid"></param>
        /// <param name="showssn"></param>
        /// <returns></returns>
        /// // GET: api/Response/ExportLoans
      //  [Authorize(Policy = "Editor")]
        [HttpGet("ExportLoans", Name = "ExportLoanHistory")]
        public IActionResult ExportLoanHistory([FromServices] FileExportProcessor fileExportProcessor, [FromQuery] int batchid = 0, [FromQuery] int studentid = 0, [FromQuery] bool showssn = false)
        {
            if (ClientProfileId == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidClientProfileId });
            }
            // no parameters passed
            if (batchid == 0 && studentid == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.InvalidNsldsRequest });
            }

            // retrieve the list of valid student requests to be exported
            var students = NsldsContext.ClientRequestStudents.Where(x =>
                ((studentid == 0 || x.Id == studentid) && (batchid == 0 || x.Link2ClientRequest == batchid)) &&
                !x.IsDeleted && x.IsReceived).ToList();

            // no valid student requests found
            if (students.Count() == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.NsldsRequestNotFound });
            }

            var ayweeks = NsldsContext.ClientProfiles
                .Single(x => x.Id == ClientProfileId).AY_Definition;

            // retrieve all students fahtype1 and fahtype5 records from database
            var sids = students.Select(x => x.Id);

            var all_fah1 = NsldsContext.FahType1Recs
                .Where(x => sids.Contains(x.Link2ClientRequestStudent)).ToList();

            var all_loans = NsldsContext.FahType5Recs
                .Where(x => sids.Contains(x.Link2ClientRequestStudent)).ToList();

            var loanHistory = new List<LoanExportRow>();
            var loanStudent = new List<LoanExportRow>();

            // loop through the student list to retrieve fah data needed
            foreach (var student in students)
            {
                var fah1 = all_fah1.FirstOrDefault(x => x.Link2ClientRequestStudent == student.Id);
                // if no fah1 data found there was a student request error
                if (fah1 == null) { continue; }

                var loans = all_loans.Where(x => x.Link2ClientRequestStudent == student.Id)
                    .OrderByDescending(x => x.LoanDate).ToList();
                // if no loans found for this student continue
                if (loans.Count() == 0) { continue; }

                // check that student has open AY loans
                var hasOpenAY = FahDef.CalcOpenAY(student.StartDate, loans, ayweeks);

                // loop through the loans and add export row
                foreach (var loan in loans)
                {
                    var isOpenAY = FahDef.N;
                    // verify open AY status of this loan
                    if (hasOpenAY == FahDef.Y && student.StartDate >= (loan.AYBeginDate ?? loan.LoanPeriodBeginDate))
                    {
                        DateTime aybegin, ayend;

                        if (loan.AYBeginDate != null && loan.AYEndDate != null)
                        {
                            int minDays = 26 * 7; // minimum weeks for AY period
                            int days = (loan.AYEndDate - loan.AYBeginDate).Value.Days;
                            if (days < minDays)
                            {
                                aybegin = loan.LoanPeriodBeginDate.Value;
                                ayend = aybegin.AddDays(minDays);
                            }
                            else
                            {
                                aybegin = loan.AYBeginDate.Value;
                                ayend = loan.AYEndDate.Value;
                            }
                        }
                        else
                        {
                            aybegin = loan.LoanPeriodBeginDate.Value;
                            ayend = aybegin.AddDays(ayweeks * 7);
                        }

                        if (student.StartDate <= ayend) { isOpenAY = FahDef.Y; }
                    }
                    var loanflag = new List<string>();
                    if (FahDef.loanFlags.Contains(loan.AdditionalUnsubLoan)) { loanflag.Add(loan.AdditionalUnsubLoan); }
                    if (loan.Reaffirmation == true) { loanflag.Add(FahDef.Reaffirmation); }

                    loanStudent.Add(new LoanExportRow
                    {
                        SSN = (showssn) ? $"*****{student.SSN.Substring(5)}" : string.Empty,
                        FirstName = student.FirstName,
                        LastName = student.LastName,
                        LoanType = loan.CodeName(x => x.LoanTypeCode, fc),
                        LoanStatus = loan.LoanStatusCode,
                        LoanFlag = (loanflag.Count > 0) ? string.Join(", ", loanflag) : string.Empty,
                        LoanStatusDate = loan.LoanStatusDate,
                        LoanAmount = loan.LoanAmount ?? 0,
                        DisbursedAmount = loan.TotalDisb ?? 0,
                        LoanDates = $"{loan.LoanPeriodBeginDate:MM/dd/yy} - {loan.LoanPeriodEndDate:MM/dd/yy}",
                        AYDates = $"{loan.AYBeginDate:MM/dd/yy} - {loan.AYEndDate:MM/dd/yy}",
                        AcademicLevel = loan.AcademicLevel,
                        School = loan.LoanSchoolCode,
                        OpenAY = isOpenAY
                    });
                }
                // if any loans in open AY, calculate the remaining total
                if (hasOpenAY == FahDef.Y)
                {
                    var oayLoans = loanStudent
                        .Where(x => x.OpenAY == hasOpenAY).Sum(x => x.LoanAmount ?? 0);
                    var lnagg = new LoanAggregateLimitCalc(fah1, true);
                    decimal gradmax = Math.Min(lnagg.gradtotal, 20500);
                    decimal totalRemain = Math.Max(gradmax - oayLoans, 0);

                    loanStudent.Where(x => x.OpenAY == hasOpenAY).ToList()
                        .ForEach(x => x.TotalRemain = totalRemain);
                }
                // add the current student loans to the history list
                loanHistory.AddRange(loanStudent);
                loanStudent.Clear();
                // add a blank row separator after each student record
                loanHistory.Add(new LoanExportRow { SSN = " " });
            }

            // no valid student loan history found
            if (loanHistory.Count() == 0)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.NsldsRequestNotFound });
            }

            fileExportProcessor.LoanHistory = loanHistory;
            var success = fileExportProcessor.ProcessLoans(User, batchid, studentid);

            var result = new
            {
                FileName = (success) ? fileExportProcessor.FileName : null,
                RouteData = (success) ? fileExportProcessor.FileRoute : null,
                Message = fileExportProcessor.ErrorMessage
            };

            return Ok(result);
        }

        #endregion

        #region Tests

        #endregion
    }
}
