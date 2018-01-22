using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileHelpers;
using System.IO;
using Global.Domain;
using NSLDS.Domain;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace NSLDS.Common
{
    public class ResponseProcessor
    {
        #region Private fields, constants and response class selector for FileRequests engine

        private Dictionary<int, string> fieldList;
        private Dictionary<int, string> errorList;

        private int GetErrorLevel(int ErrorCode)
        {
            switch (ErrorCode)
            {
                case 1:
                case 2:
                case 4:
                case 5:
                case 6:
                case 7:
                case 33:
                case 35:
                case 18:
                case 26:
                case 32:
                case 34:
                case 19:
                case 21:
                case 22:
                case 31:
                    return 1; // batch fail error
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 23:
                case 24:
                case 25:
                case 28:
                case 100:
                case 101:
                    return 2; // student request error
                case 701:
                case 702:
                case 703:
                case 704:
                case 705:
                case 706:
                    return 3; // student request warning
                default:
                    return 99; // unknown error
            }
        }

        private static class Ident
        {
            public const string
                TRNINFOP = "TRNINFOP",
                FAHEXTOP = "FAHEXTOP",
                TRALRTOP = "TRALRTOP",
                TDClientHeader = "O*N05",
                TDClientTrailer = "O*N95",
                NsldsTsmHeader = "0TSM ALERT",
                NsldsAckHeader = "0TSM/FAH",
                NsldsAckRecord = "1",
                NsldsAckTrailer = "9TSM/FAH",
                NsldsFahHeader = "0",
                NsldsFahTrailer = "9",
                NsldsFahType1Rec = "1",
                NsldsFahType2Rec = "2",
                NsldsFahType3Rec = "3",
                NsldsFahType4Rec = "4",
                NsldsFahType5Rec = "5";
        }

        private Type[] AckTypes =
        {
                typeof(TDClientHeader),
                typeof(TDClientTrailer),
                typeof(NsldsAckHeader),
                typeof(NsldsAckTrailer),
                typeof(NsldsAckRecord)
            };

        private Type[] FahTypes =
        {
                typeof(TDClientHeader),
                typeof(TDClientTrailer),
                typeof(NsldsFahHeader),
                typeof(NsldsFahTrailer),
                typeof(NsldsFahType1Rec),
                typeof(NsldsFahType2Rec),
                typeof(NsldsFahType3Rec),
                typeof(NsldsFahType4Rec),
                typeof(NsldsFahType5Rec)
            };

        private Type AckSelector(MultiRecordEngine engine, string recordLine)
        {
            if (recordLine.Length == 0) return null;
            if (recordLine.StartsWith(Ident.TDClientHeader)) return typeof(TDClientHeader);
            if (recordLine.StartsWith(Ident.TDClientTrailer)) return typeof(TDClientTrailer);
            if (recordLine.StartsWith(Ident.NsldsAckHeader)) return typeof(NsldsAckHeader);
            if (recordLine.StartsWith(Ident.NsldsAckRecord)) return typeof(NsldsAckRecord);
            if (recordLine.StartsWith(Ident.NsldsAckTrailer)) return typeof(NsldsAckTrailer);

            else return null;
        }

        private Type FahSelector(MultiRecordEngine engine, string recordLine)
        {
            if (recordLine.Length == 0) return null;
            if (recordLine.StartsWith(Ident.TDClientHeader)) return typeof(TDClientHeader);
            if (recordLine.StartsWith(Ident.TDClientTrailer)) return typeof(TDClientTrailer);
            if (recordLine.StartsWith(Ident.NsldsFahHeader)) return typeof(NsldsFahHeader);
            if (recordLine.StartsWith(Ident.NsldsFahTrailer)) return typeof(NsldsFahTrailer);
            if (recordLine.StartsWith(Ident.NsldsFahType1Rec)) return typeof(NsldsFahType1Rec);
            if (recordLine.StartsWith(Ident.NsldsFahType2Rec)) return typeof(NsldsFahType2Rec);
            if (recordLine.StartsWith(Ident.NsldsFahType3Rec)) return typeof(NsldsFahType3Rec);
            if (recordLine.StartsWith(Ident.NsldsFahType4Rec)) return typeof(NsldsFahType4Rec);
            if (recordLine.StartsWith(Ident.NsldsFahType5Rec)) return typeof(NsldsFahType5Rec);

            else return null;
        }

        #endregion

        #region Protected Methods

        protected bool ProcessAckRecords(int id = 0)
        {
            Errors.Clear();
            ErrorMessage.Clear();
            HasErrors = false;
            Skip = false;
            bool success = true;
            int errorLevel = 0;
            string opeId = string.Empty;
            int batchId = 0;
            short seq = 0;
            ClientRequest cReq = null;
            ClientRequestAlert cReqAlert = null;
            int curBatchId = 0;
            string ssn = string.Empty;
            string reqType = string.Empty;
            List<ClientRequestStudent> cReqS = null;
            List<object> error = new List<object>();
            bool isError = false;

            // process & save acknowledgement records
            foreach (var item in Results)
            {
                if (item is TDClientHeader) // process tdclient header
                {
                    TDClientHeader a = (TDClientHeader)item;
                    //opeId = a.OpeId;
                    //batchId = a.BatchId;
                }
                else if (item is TDClientTrailer) // process tdclient trailer
                {
                    opeId = string.Empty;
                    batchId = 0;
                }
                else if (item is NsldsAckHeader) // process ack header
                {
                    NsldsAckHeader a = (NsldsAckHeader)item;
                    opeId = a.OPEID;
                    var cpId = NsldsContext.ClientProfiles
                        .SingleOrDefault(x => x.OPEID == opeId)?.Id;
                    
                    // no client profile OPEID match?
                    if (cpId == null)
                    {
                        success = false;
                        isError = true;
                        error.Clear();
                        error.Add(item);
                        ErrorMessage.Add($"No client profile found for OPEID {opeId}, batch {a.SubmittedOn.ToShortDateString()}-{a.Sequence:D2}");
                        continue;
                    }

                    seq = a.Sequence;
                    // datachk-110: if processing a queued batch, use the id parameter
                    if (id > 0)
                    {
                        cReq = NsldsContext.ClientRequests
                            .SingleOrDefault(x => x.Id == id);
                    }
                    else
                    {
                        cReq = NsldsContext.ClientRequests
                            .SingleOrDefault(x => x.Link2ClientProfile == cpId &&
                            x.SubmittedOn.HasValue &&
                            x.SubmittedOn.Value.Date == a.SubmittedOn &&
                            x.Sequence == seq);
                    }

                    // request not found, save the batch records to be moved to error folder
                    if (cReq == null)
                    {
                        success = false;
                        isError = true;
                        error.Clear();
                        error.Add(item);
                        ErrorMessage.Add($"Batch request not found for {a.SubmittedOn.ToShortDateString()}-{a.Sequence:D2}");
                        continue;
                    } 
                    if(cReq.ReceivedOn.HasValue) { cReq = null;  continue; } // skip already processed

                    batchId = cReq.Id;
                    cReqAlert = new ClientRequestAlert
                    {
                        Link2ClientRequest = batchId
                    };

                    cReqS = NsldsContext.ClientRequestStudents
                        .Where(x => x.Link2ClientRequest == batchId &&
                            !x.IsDeleted && x.IsSubmitted && !x.ReceivedOn.HasValue)
                        .ToList();
                }
                else if (item is NsldsAckTrailer) // process ack trailer
                {
                    if (isError)
                    {
                        error.Add(item);
                        Errors.Add(error.ToList());
                        error.Clear();
                        isError = false;
                    }
                    if (cReq == null) { continue; }
                    NsldsAckTrailer a = (NsldsAckTrailer)item;
                    // populate and add Alert record
                    cReqAlert.ErrorCount = a.ErrorCount;
                    cReqAlert.ErrorRecCount = a.ErrorRecCount;
                    cReqAlert.RecordCount = a.RecordCount;
                    cReqAlert.WarningCount = a.WarningCount;
                    cReqAlert.WarningRecCount = a.WarningRecCount;
                    NsldsContext.ClientRequestAlerts.Add(cReqAlert);
                    // update client request record
                    cReq.ReceivedOn = DateTime.Now;
                    NsldsContext.Entry(cReq).State = EntityState.Modified;
                    // update received date of all submitted students
                    //var students = NsldsContext.ClientRequestStudents
                    //    .Where(x => x.Link2ClientRequest == batchId && !x.IsDeleted 
                    //    && x.IsSubmitted && !x.ReceivedOn.HasValue)
                        //.OrderBy(o => o.Id)
                        //.GroupBy(g => new { g.SSN, g.RequestType, g.StartDate })
                        //.Select(s => s.First())
                        //.ToList();

                    foreach (var student in cReqS)
                    {
                        student.ReceivedOn = DateTime.Now;
                        NsldsContext.Entry(student).State = EntityState.Modified;
                    }
                    // datachk-166: save all database changes for this batch
                    NsldsContext.SaveChanges();

                    seq = 0;
                    errorLevel = 0;
                    curBatchId = 0;
                    ssn = string.Empty;
                    reqType = string.Empty;
                    cReq = null;
                    cReqS = null;
                    cReqAlert = null;
                }
                else if (item is NsldsAckRecord) // process ack record
                {
                    if (isError) { error.Add(item); }
                    if (cReq == null) { continue; }
                    NsldsAckRecord a = (NsldsAckRecord)item;
                    string e = errorList[a.ErrorCode];
                    string f = fieldList[a.FieldInError];
                    errorLevel = GetErrorLevel(a.ErrorCode);
                    // batch error
                    if (errorLevel == 1)
                    {
                        cReq.IsReceived = true;
                        cReq.IsFailed = true;
                        cReqAlert.ErrorCode = a.ErrorCode;
                        cReqAlert.ErrorLevel = errorLevel;
                        cReqAlert.ErrorMessage = e;
                        cReqAlert.FieldInError = a.FieldInError;
                        cReqAlert.FieldName = f;
                        cReqAlert.FieldValue = a.FieldValue;
                    }
                    // student request error or warning
                    else if (errorLevel == 2 || errorLevel == 3)
                    {
                        // only retrieve student request if not already loaded
                        if (curBatchId != batchId || ssn != a.SSN || reqType != a.RequestType)
                        {
                            curBatchId = batchId;
                            ssn = a.SSN;
                            reqType = a.RequestType;
                        }
                        // should not happen unless db record manually changed
                        if (cReqS.Count() == 0) { continue; }
                        // add the alert to all records for this student
                        foreach (var s in cReqS.Where(x => x.SSN == ssn && x.RequestType == reqType))
                        {
                            // create alert record
                            ClientRequestStudentAlert cReqSAlert = new ClientRequestStudentAlert
                            {
                                Link2ClientRequestStudent = s.Id,
                                ErrorCode = a.ErrorCode,
                                ErrorLevel = errorLevel,
                                ErrorMessage = e,
                                FieldInError = a.FieldInError,
                                FieldName = f,
                                FieldValue = a.FieldValue
                            };
                            NsldsContext.ClientRequestStudentAlerts.Add(cReqSAlert);
                        }
                    }
                }
            }
            return success;
        }

        protected bool ProcessFahRecords(int id = 0)
        {
            Errors.Clear();
            ErrorMessage.Clear();
            HasErrors = false;
            Skip = false;
            bool success = true;
            string opeId = string.Empty;
            int batchId = 0;
            short seq = 0;
            string ssn = string.Empty;
            List<string> ssnlist = new List<string>();
            bool ssndone = false;
            string reqTypeT = "T";
            ClientRequest cReq = null;
            List<ClientRequestStudent> cReqS = null;
            List<object> error = new List<object>();
            bool isError = false;

            // initialize & configure automapper data transfer classes
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<NsldsFahType1Rec, NsldsFAHType1>();
                cfg.CreateMap<NsldsFahType2Rec, NsldsFAHType2>();
                cfg.CreateMap<NsldsFahType3Rec, NsldsFAHType3>();
                cfg.CreateMap<NsldsFahType4Rec, NsldsFAHType4>();
                cfg.CreateMap<NsldsFahType5Rec, NsldsFAHType5>();
            });

            // process & save financial history records
            foreach (var item in Results)
            {
                if (item is TDClientHeader) // process tdclient header
                {
                    TDClientHeader a = (TDClientHeader)item;
                    //opeId = a.OpeId;
                    //batchId = a.BatchId;
                }
                else if (item is TDClientTrailer) // process tdclient trailer
                {
                    opeId = string.Empty;
                    batchId = 0;
                }
                else if (item is NsldsFahHeader) // process fah header
                {
                    NsldsFahHeader a = (NsldsFahHeader)item;
                    opeId = a.OpeId;
                    var cpId = NsldsContext.ClientProfiles
                        .SingleOrDefault(x => x.OPEID == opeId)?.Id;
                    
                    // no client profile OPEID match?
                    if (cpId == null)
                    {
                        success = false;
                        isError = true;
                        error.Clear();
                        error.Add(item);
                        ErrorMessage.Add($"No client profile found for OPEID {opeId}, batch {a.Date.ToShortDateString()}-{a.Sequence:D2}");
                        continue;
                    }

                    seq = a.Sequence;
                    
                    // datachk-110: if processing a queued batch, use the id parameter
                    if (id > 0)
                    {
                        cReq = NsldsContext.ClientRequests
                            .SingleOrDefault(x => x.Id == id);
                    }
                    else
                    { 
                        cReq = NsldsContext.ClientRequests.SingleOrDefault(x => 
                            x.Link2ClientProfile == cpId &&
                            x.Sequence == seq &&
                            x.SubmittedOn.HasValue &&
                            x.SubmittedOn.Value.Date == a.Date);
                    }

                    // request not found, save the batch records to be moved to error folder
                    if (cReq == null)
                    {
                        success = false;
                        isError = true;
                        error.Clear();
                        error.Add(item);
                        ErrorMessage.Add($"Batch request not found for {a.Date.ToShortDateString()}-{a.Sequence:D2}");
                        continue;
                    }
                    // ensure the errors & warnings from trninfop.dat has already run
                    if (!cReq.ReceivedOn.HasValue) { Skip = true; cReq = null; continue; } // skip not processed
                    // already processed, just skip
                    if (cReq.IsReceived) { cReq = null; continue; }

                    batchId = cReq.Id;
                    // tracking duplicate ssn records for a given batch

                    cReqS = NsldsContext.ClientRequestStudents
                        .Where(x => x.Link2ClientRequest == batchId 
                            && !x.IsDeleted && x.IsSubmitted && !x.IsReceived && x.ReceivedOn.HasValue)
                        .ToList();
                }
                else if (item is NsldsFahTrailer) // process ack trailer
                {
                    if (isError)
                    {
                        error.Add(item);
                        Errors.Add(error.ToList());
                        error.Clear();
                        isError = false;
                    }
                    if (cReq == null) { continue; }
                    NsldsFahTrailer a = (NsldsFahTrailer)item;

                    // update received status of all submitted students for this batch
                    //var students = NsldsContext.ClientRequestStudents
                    //    .Where(x => x.Link2ClientRequest == batchId && !x.IsDeleted && x.IsSubmitted)
                    //    .OrderBy(o => o.Id)
                    //    .GroupBy(g => new { g.SSN, g.RequestType, g.StartDate })
                    //    .Select(s => s.First())
                    //    .ToList();

                    foreach (var student in cReqS)
                    {
                        student.ReceivedOn = DateTime.Now;
                        student.IsReceived = true;
                        student.Response = string.Format(RouteConstant.Response, student.Id);
                        NsldsContext.Entry(student).State = EntityState.Modified;
                    }
                    // update client request record
                    cReq.IsReceived = true;
                    cReq.ReceivedOn = DateTime.Now;
                    if (string.IsNullOrEmpty(cReq.Response)) { cReq.Response = StatusConstant.Received; }
                    NsldsContext.Entry(cReq).State = EntityState.Modified;
                    // save this batch to database
                    NsldsContext.SaveChanges();

                    batchId = 0;
                    seq = 0;
                    ssn = string.Empty;
                    ssnlist.Clear();
                    ssndone = false;
                    cReq = null;
                    cReqS = null;
                }
                else if (item is NsldsFahType1Rec) // begin new student history
                {
                    if (isError) { error.Add(item); }
                    if (cReq == null) { continue; }

                    // datachk-166: save previous student records to database (transaction)
                    if (!string.IsNullOrEmpty(ssn)) { NsldsContext.SaveChanges(); }

                    ssn = ((NsldsFahType1Rec)item).SSN;

                    // check if this ssn record has already been processed
                    ssndone = ssnlist.Any(x => x == ssn);
                    if (!ssndone) { ssnlist.Add(ssn); } else { continue; }

                    // shouldn't happen unless db manually modified
                    if (cReqS.Count() == 0) { cReqS = null; continue; }
                    foreach (var s in cReqS.Where(x => x.SSN == ssn && x.RequestType != reqTypeT))
                    {
                        var type1 = Mapper.Map<NsldsFAHType1>(item);
                        type1.Link2ClientRequestStudent = s.Id;
                        NsldsContext.FahType1Recs.Add(type1);
                    }
                }
                else if (item is NsldsFahType2Rec)
                {
                    if (isError) { error.Add(item); }
                    if (cReq == null || cReqS == null || ssndone) { continue; }
                    foreach (var s in cReqS.Where(x => x.SSN == ssn && x.RequestType != reqTypeT))
                    {
                        var type2 = Mapper.Map<NsldsFAHType2>(item);
                        type2.Link2ClientRequestStudent = s.Id;
                        NsldsContext.FahType2Recs.Add(type2);
                    }
                }
                else if (item is NsldsFahType3Rec)
                {
                    if (isError) { error.Add(item); }
                    if (cReq == null || cReqS == null || ssndone) { continue; }
                    foreach (var s in cReqS.Where(x => x.SSN == ssn && x.RequestType != reqTypeT))
                    {
                        var type3 = Mapper.Map<NsldsFAHType3>(item);
                        type3.Link2ClientRequestStudent = s.Id;
                        NsldsContext.FahType3Recs.Add(type3);
                    }
                }
                else if (item is NsldsFahType4Rec)
                {
                    if (isError) { error.Add(item); }
                    if (cReq == null || cReqS == null || ssndone) { continue; }
                    foreach (var s in cReqS.Where(x => x.SSN == ssn && x.RequestType != reqTypeT))
                    {
                        var type4 = Mapper.Map<NsldsFAHType4>(item);
                        type4.Link2ClientRequestStudent = s.Id;
                        NsldsContext.FahType4Recs.Add(type4);
                    }
                }
                else if (item is NsldsFahType5Rec)
                {
                    if (isError) { error.Add(item); }
                    if (cReq == null || cReqS == null || ssndone) { continue; }
                    foreach (var s in cReqS.Where(x => x.SSN == ssn && x.RequestType != reqTypeT))
                    {
                        var type5 = Mapper.Map<NsldsFAHType5>(item);
                        type5.Link2ClientRequestStudent = s.Id;
                        NsldsContext.FahType5Recs.Add(type5);
                    }
                }
            }
            return success;
        }

        // datachk-68: create batch request + students from response file
        protected ClientRequest CreateRequestFromResults(string OpeId, string user, bool isTM, DateTime startDate, int batchId = 0)
        {
            ClientRequest request = null;

            if (batchId > 0)
            {
                request = NsldsContext.ClientRequests.SingleOrDefault(x => x.Id == batchId);
                if (request != null) { return request; }
            }

            foreach (var item in Results)
            {
                if (item is NsldsFahHeader)
                {
                    var a = item as NsldsFahHeader;
                    // check that this response date/sequence doesn't already exist
                    var cpId = NsldsContext
                        .ClientProfiles.Single(x => x.OPEID == OpeId).Id;

                    var exist = NsldsContext.ClientRequests.Any(
                        x => x.Link2ClientProfile == cpId &&
                        x.SubmittedOn.Value.Date == a.Date &&
                        x.Sequence == a.Sequence);

                    if (exist)
                    {
                        // retrieve the last sequence# for submitted batch requests
                        short seq = NsldsContext.ClientRequests
                            .Where(x => x.Link2ClientProfile == cpId && x.SubmittedOn.HasValue &&
                                x.SubmittedOn.Value.Date == a.Date && x.IsSubmitted)
                            .Max(x => x.Sequence as short?) ?? 0;
                        seq++;
                        a.Sequence = seq;
                    }

                    // create new request
                    request = new ClientRequest
                    {
                        Link2ClientProfile = cpId,
                        RevOn = DateTime.Now,
                        RevBy = user,
                        SubmittedOn = a.Date,
                        Sequence = a.Sequence,
                        IsSubmitted = true,
                        IsReceived = false,
                        ReceivedOn = DateTime.Now,
                        IsDeleted = false,
                        IsFailed = false,
                        IsOnHold = false,
                        IsValid = true,
                        IsTM = isTM
                    };
                    // add request to database to retrieve the BatchId
                    NsldsContext.ClientRequests.Add(request);
                    NsldsContext.SaveChanges();
                }
                else if (item is NsldsFahType1Rec)
                {
                    var a = item as NsldsFahType1Rec;

                    bool? isPellReviewed = null;
                    bool? isLoanReviewed = null;
                    bool? isTeachReviewed = null;
                    bool? isGrantReviewed = null;
                    // assign change alert review flags for TM response
                    if (isTM)
                    {
                        if (a.PellChange.HasValue && a.PellChange.Value)
                        { isPellReviewed = false; }

                        if ((a.AggregateChange.HasValue && a.AggregateChange.Value) ||
                            (a.LoanChange.HasValue && a.LoanChange.Value))
                        { isLoanReviewed = false; }

                        if (a.TeachLoanChange.HasValue && a.TeachLoanChange.Value)
                        { isTeachReviewed = false; }
                        
                        // datachk-217: added TeachGrantDataChange to grant review flag
                        if ((a.ACGChange.HasValue && a.ACGChange.Value) ||
                            (a.SMARTChange.HasValue && a.SMARTChange.Value) ||
                            (a.TeachGrantDataChange.HasValue && a.TeachGrantDataChange.Value) ||
                            (a.TeachGrantChange.HasValue && a.TeachGrantChange.Value))
                        { isGrantReviewed = false; }
                    }

                    var student = new ClientRequestStudent
                    {
                        Link2ClientRequest = request.Id,
                        DOB = a.DOB,
                        FirstName = a.FirstName,
                        LastName = a.LastName,
                        RequestType = RequestType.History,
                        SSN = a.SSN,
                        StartDate = startDate,
                        RevOn = DateTime.Now,
                        RevBy = user,
                        SubmittedOn = request.SubmittedOn,
                        IsSubmitted = true,
                        ReceivedOn = DateTime.Now,
                        IsDeleted = false,
                        IsReceived = false,
                        IsResolved = false,
                        IsValid = true,
                        EnrollBeginDate = startDate,
                        IsPellReviewed = isPellReviewed,
                        IsLoanReviewed = isLoanReviewed,
                        IsTeachReviewed = isTeachReviewed,
                        IsGrantReviewed = isGrantReviewed,
                        IsRefreshed = false
                    };
                    NsldsContext.ClientRequestStudents.Add(student);
                }
            }
            // save all student records
            NsldsContext.SaveChanges();

            return request;
        }

        #endregion

        #region Public properties

        public object[] Results { get; private set; }
        public List<List<object>> Errors { get; private set; } = new List<List<object>>();
        public List<string> ErrorMessage { get; set; } = new List<string>();
        public string ErrorPath { get; set; }
        public bool HasErrors { get; set; } = false;
        public bool Skip { get; set; } = false;
        public GlobalContext GlobalContext { get; set; }
        public NSLDS_Context NsldsContext { get; set; }

        #endregion

        #region Constructor

        public ResponseProcessor()
        {
            // initialize private dictionaries
            fieldList = new Dictionary<int, string>();
            errorList = new Dictionary<int, string>();
            // add field codes/values
            fieldList.Add(202, "SSN");
            fieldList.Add(203, "FirstName");
            fieldList.Add(204, "LastName");
            fieldList.Add(205, "DOB");
            fieldList.Add(206, "EnrollBeginDate");
            fieldList.Add(207, "MonitorBeginDate");
            fieldList.Add(208, "DeleteMonitoring");
            fieldList.Add(209, "OPEID");
            fieldList.Add(210, "RequestType");
            // add error codes/values
            errorList.Add(1, "No Alert Profile established");
            errorList.Add(2, "No header record");
            errorList.Add(4, "School/Branch does not exist in NSLDS");
            errorList.Add(5, "Servicer not authorized");
            errorList.Add(33, "TG Number not valid for school");
            errorList.Add(35, "User not authorized");
            errorList.Add(6, "Submittal Date is not a valid date");
            errorList.Add(7, "Submittal Sequence Number is not numeric");
            errorList.Add(9, "SSN is not numeric");
            errorList.Add(10, "First name is all spaces");
            errorList.Add(11, "Last name is all spaces");
            errorList.Add(12, "Date of Birth is an invalid date");
            errorList.Add(13, "Enrollment Begin date is an invalid date");
            errorList.Add(14, "Enrollment Begin date is older than 90 days");
            errorList.Add(15, "Enrollment Begin date is greater than current date plus 18 months (548 days)");
            errorList.Add(16, "Monitor Begin date is an invalid date");
            errorList.Add(23, "Enrollment Begin date is current or a future date and Monitor Begin date is greater than Enrollment Begin Date");
            errorList.Add(24, "Monitor Begin date is older than 18 months (548 days)");
            errorList.Add(25, "Monitor Begin date is greater than current date plus 18 months (548 days)");
            errorList.Add(17, "Delete From List Indicator is not Y or N");
            errorList.Add(18, "School/Branch does not exist in NSLDS");
            errorList.Add(26, "School/Branch on detail record does not match School/Branch on header record");
            errorList.Add(28, "Request indicator is not H, T, or B");
            errorList.Add(32, "Submittal Control / Submittal Control Detail not found");
            errorList.Add(34, "Submittal Control / Submittal Control Detail not found");
            errorList.Add(100, "Match on SSN. No match on DOB, First Name, Last Name");
            errorList.Add(101, "Student SSN not found");
            errorList.Add(19, "No trailer record");
            errorList.Add(31, "Submittal file not found");
            errorList.Add(21, "Trailer record count does not match number of detail records");
            errorList.Add(22, "Trailer record count not numeric");
            errorList.Add(701, "Student SSN not found. (Student added to Monitoring List, but is not on Database)");
            errorList.Add(702, "Match on SSN. No match on DOB, First Name, Last Name. (Student added to Monitoring List, but does not match information on Database)");
            errorList.Add(703, "Delete requested for Student not on Monitoring List");
            errorList.Add(704, "Enrollment Begin Date is a past date; Monitor Begin date changed to current date");
            errorList.Add(705, "Successful Match. Student has no selectable loans");
            errorList.Add(706, "Successful Match. Student has no selectable loans and grants");
        }

        #endregion

        #region Public Methods

        public bool Run(string fileName, string OpeId, int batchId = 0)
        {
            bool success = false;

            // check that file exists
            if (!File.Exists(fileName)) return success;

            var fileType = Path.GetFileNameWithoutExtension(fileName).ToUpper();

            Type[] curTypes = null;
            RecordTypeSelector curSelector = null;

            if (fileType == Ident.TRNINFOP)
            {
                curTypes = AckTypes;
                curSelector = new RecordTypeSelector(AckSelector);
            }
            else if (fileType == Ident.FAHEXTOP || fileType == Ident.TRALRTOP)
            {
                curTypes = FahTypes;
                curSelector = new RecordTypeSelector(FahSelector);
            }
            else
            {
                ErrorMessage.Add($"{Path.GetFileName(fileName)} is not a recognized filename. Valid filenames are TRNINFOP.xxx, FAHEXTOP.xxx or TRALRTOP.xxx");
                return success;
            }
            // Instantiate the engine
            using (var engine = new MultiRecordEngine(curSelector, curTypes))
            {
                Results = engine.ReadFile(fileName);

                if (fileType == Ident.TRNINFOP)
                {
                    success = ProcessAckRecords(batchId);
                }
                else if (fileType == Ident.FAHEXTOP)
                {
                    success = ProcessFahRecords(batchId);
                }
                else if (fileType == Ident.TRALRTOP)
                {
                    var request = CreateRequestFromResults(OpeId, "TdClient Scheduler", true, DateTime.Today, batchId);
                    success = ProcessFahRecords(request.Id);
                }

                // check the errors list
                if (Errors.Count() > 0)
                {
                    HasErrors = true;
                    foreach (var error in Errors)
                    {
                        var errorFile = Path.Combine(ErrorPath, $"{fileType}.{DateTime.Now.Ticks}");
                        engine.WriteFile(errorFile, error);
                        error.Clear();
                    }
                    Errors.Clear();
                }
            }
            return success;
        }

        public List<ResponseBatchResult> Analyse(string opeId, string file)
        {
            try
            {
                HasErrors = false;
                var result = new List<ResponseBatchResult>();

                // we'll save individual batch response separately for processing once confirmed
                var pendingBatch = new List<object>();
                var pendingPath = Path.Combine(Path.GetDirectoryName(file), "pending");
                if (!Directory.Exists(pendingPath)) { Directory.CreateDirectory(pendingPath); }
                var pendingFile = string.Empty;

                // datachk-165: determine file type from headers, not filename
                var fileType = string.Empty;
                using (StreamReader reader = File.OpenText(file))
                {
                    // if tdclient header present, read the message class
                    var line1 = reader.ReadLine();
                    if (line1.StartsWith(Ident.TDClientHeader))
                    {
                        fileType = line1.Substring(24, 8);
                    }
                    // edconnect header
                    else if (line1.StartsWith(Ident.NsldsAckHeader))
                    {
                        fileType = Ident.TRNINFOP;
                    }
                    else if (line1.StartsWith(Ident.NsldsTsmHeader))
                    {
                        fileType = Ident.TRALRTOP;
                    }
                    else if (line1.StartsWith(Ident.NsldsFahHeader))
                    {
                        fileType = Ident.FAHEXTOP;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }

                Type[] curTypes = null;
                RecordTypeSelector curSelector = null;

                if (fileType == Ident.TRNINFOP)
                {
                    curTypes = AckTypes;
                    curSelector = new RecordTypeSelector(AckSelector);
                }
                else if (fileType == Ident.FAHEXTOP || fileType == Ident.TRALRTOP)
                {
                    curTypes = FahTypes;
                    curSelector = new RecordTypeSelector(FahSelector);
                }
                // Instantiate the engine
                using (var engine = new MultiRecordEngine(curSelector, curTypes))
                {
                    // read response file of any allowed type
                    // we only need header/trailer info to analyse
                    // ignore exceptions, process the file
                    //engine.ErrorMode = ErrorMode.IgnoreAndContinue;
                    Results = engine.ReadFile(file);

                    ResponseBatchResult batchInfo = new ResponseBatchResult();
                    int count = 0;

                    foreach (var item in Results)
                    {
                        if (item is NsldsAckHeader)
                        {
                            var a = item as NsldsAckHeader;
                            // check that OPEID is the correct one for this response
                            if (a.OPEID != opeId) { throw new Exception(); }

                            batchInfo = new ResponseBatchResult
                            {
                                SubmittedOn = a.SubmittedOn,
                                Sequence = a.Sequence
                            };
                            pendingBatch.Clear();
                            pendingBatch.Add(item);
                            pendingFile = $"{fileType}.{a.SubmittedOn.ToString("yyyyMMdd")}{a.Sequence:D2}";
                        }
                        else if (item is NsldsFahHeader)
                        {
                            var a = item as NsldsFahHeader;
                            // check that OPEID is the correct one for this response
                            if (a.OpeId != opeId) { throw new Exception(); }

                            count = 0; //initialize the student count (type1rec)
                            batchInfo = new ResponseBatchResult
                            {
                                SubmittedOn = a.Date,
                                Sequence = a.Sequence
                            };
                            pendingBatch.Clear();
                            pendingBatch.Add(item);
                            pendingFile = $"{fileType}.{a.Date.ToString("yyyyMMdd")}{a.Sequence:D2}";
                        }
                        else if (item is NsldsAckTrailer)
                        {
                            var a = item as NsldsAckTrailer;
                            batchInfo.RecordCount = a.RecordCount;
                            batchInfo.FileName = fileType;
                            result.Add(batchInfo);
                            pendingBatch.Add(item);
                            engine.WriteFile(Path.Combine(pendingPath, pendingFile), pendingBatch);
                        }
                        else if (item is NsldsFahTrailer)
                        {
                            var a = item as NsldsFahTrailer;
                            batchInfo.RecordCount = count;
                            batchInfo.FileName = fileType;
                            result.Add(batchInfo);
                            pendingBatch.Add(item);
                            engine.WriteFile(Path.Combine(pendingPath, pendingFile), pendingBatch);
                        }
                        else if (item is NsldsFahType1Rec)
                        {
                            count++;
                            pendingBatch.Add(item);
                        }
                        else if (item is NsldsAckRecord ||
                            item is NsldsFahType2Rec ||
                            item is NsldsFahType3Rec ||
                            item is NsldsFahType4Rec ||
                            item is NsldsFahType5Rec)
                        {
                            pendingBatch.Add(item);
                        }
                    }
                }
                return result;
            }
            catch
            {
                HasErrors = true;
                return null;
            }
        }

        public string Process(ResponseBatchResult batch, string RootPath, string OpeId, string user)
        {
            HasErrors = false;
            try
            {
                // configure various working paths
                var queuePath = Path.Combine(RootPath, OpeId, batch.UploadDate, "queue\\pending");
                var pendingPath = Path.Combine(RootPath, OpeId, batch.UploadDate, batch.UploadId, "pending");
                var processedPath = Path.Combine(RootPath, OpeId, batch.UploadDate, batch.UploadId, "processed");
                if (!Directory.Exists(processedPath)) { Directory.CreateDirectory(processedPath); }
                if (!Directory.Exists(queuePath)) { Directory.CreateDirectory(queuePath); }

                // load the expected pending response files for this batch
                var allowedFiles = batch.FileName.Split('/');
                var pattern = $"*.{batch.SubmittedOn.ToString("yyyyMMdd")}{batch.Sequence:D2}";
                var pendingFiles = Directory.GetFiles(pendingPath, pattern).ToList();
                foreach (var file in pendingFiles.ToList())
                {
                    if (!allowedFiles.Contains(Path.GetFileNameWithoutExtension(file), StringComparer.InvariantCultureIgnoreCase))
                    {
                        pendingFiles.Remove(file);
                    }
                }
                // unlikely to happen
                if (pendingFiles.Count() == 0)
                {
                    HasErrors = true;
                    return MessageConstant.NoResponseFiles;
                }
                // retrieve the trninfop, fahextop or tralrtop responses if there
                var trninfop = pendingFiles
                    .SingleOrDefault(x => Path.GetFileNameWithoutExtension(x).ToUpper() == Ident.TRNINFOP);
                var fahextop = pendingFiles
                    .SingleOrDefault(x => Path.GetFileNameWithoutExtension(x).ToUpper() == Ident.FAHEXTOP);
                var tralrtop = pendingFiles
                    .SingleOrDefault(x => Path.GetFileNameWithoutExtension(x).ToUpper() == Ident.TRALRTOP);


                ClientRequest request = null;

                // request already exists
                if (batch.BatchId > 0 && tralrtop == null)
                {
                    request = NsldsContext.ClientRequests
                        .SingleOrDefault(x => x.Id == batch.BatchId);
                    if (request == null)
                    {
                        HasErrors = true;
                        return string.Format(MessageConstant.BatchNotFound, batch.BatchId);
                    }
                    if (request.IsReceived)
                    {
                        HasErrors = true;
                        return string.Format(MessageConstant.BatchAlreadyProcessed, batch.BatchId);
                    }
                    if (!request.ReceivedOn.HasValue && trninfop == null)
                    {
                        HasErrors = true;
                        return string.Format(MessageConstant.BatchRequiresResponse, batch.BatchId, Ident.TRNINFOP);
                    }
                    if (request.ReceivedOn.HasValue && fahextop == null)
                    {
                        HasErrors = true;
                        return string.Format(MessageConstant.BatchRequiresResponse, batch.BatchId, Ident.FAHEXTOP);
                    }
                    // start processing response files
                    // retrieve list of student ssn for this request to match response
                    var ssnList = NsldsContext.ClientRequestStudents
                        .Where(x => x.Link2ClientRequest == request.Id && !x.IsDeleted)
                        .Select(x => x.SSN).Distinct().ToList();

                    if (trninfop != null)
                    {
                        var curSelector = new RecordTypeSelector(AckSelector);
                        // Instantiate the engine
                        using (var engine = new MultiRecordEngine(curSelector, AckTypes))
                        {
                            // read response file of any allowed type
                            Results = engine.ReadFile(trninfop);
                            foreach (var item in Results)
                            {
                                if (item is NsldsAckHeader)
                                {
                                    var a = item as NsldsAckHeader;
                                    if (a.SubmittedOn != request.SubmittedOn.Value.Date ||
                                        a.Sequence != request.Sequence)
                                    {
                                        // these need to match for processing
                                        a.SubmittedOn = request.SubmittedOn.Value.Date;
                                        a.Sequence = request.Sequence;
                                    }
                                }
                                else if (item is NsldsAckRecord)
                                {
                                    var a = item as NsldsAckRecord;
                                    if (!ssnList.Contains(a.SSN))
                                    {
                                        HasErrors = true;
                                        return string.Format(MessageConstant.BatchRecordNotFound, Ident.TRNINFOP, a.FirstName, a.LastName);
                                    }
                                }
                            }
                            // ready to process
                            var newFile = $"{Path.GetFileNameWithoutExtension(trninfop)}.{batch.BatchId}";
                            engine.WriteFile(Path.Combine(queuePath, newFile), Results);
                            if (HasErrors)
                            {
                                return string.Format(MessageConstant.ResponseErrorFound, Ident.TRNINFOP);
                            }
                        }
                    }
                    if (fahextop != null)
                    {
                        var curSelector = new RecordTypeSelector(FahSelector);
                        // Instantiate the engine
                        using (var engine = new MultiRecordEngine(curSelector, FahTypes))
                        {
                            // read response file of any allowed type
                            Results = engine.ReadFile(fahextop);
                            foreach (var item in Results)
                            {
                                if (item is NsldsFahHeader)
                                {
                                    var a = item as NsldsFahHeader;
                                    if (a.Date != request.SubmittedOn.Value.Date ||
                                        a.Sequence != request.Sequence)
                                    {
                                        // these need to match for processing
                                        a.Date = request.SubmittedOn.Value.Date;
                                        a.Sequence = request.Sequence;
                                    }
                                }
                                else if (item is NsldsFahType1Rec)
                                {
                                    var a = item as NsldsFahType1Rec;
                                    if (!ssnList.Contains(a.SSN))
                                    {
                                        HasErrors = true;
                                        return string.Format(MessageConstant.BatchRecordNotFound, Ident.FAHEXTOP, a.FirstName, a.LastName);
                                    }
                                }
                            }
                            // ready to process
                            var newFile = $"{Path.GetFileNameWithoutExtension(fahextop)}.{batch.BatchId}";
                            engine.WriteFile(Path.Combine(queuePath, newFile), Results);
                            if (HasErrors)
                            {
                                return string.Format(MessageConstant.ResponseErrorFound, Ident.FAHEXTOP);
                            }
                        }
                    }
                }
                else // new batch request to be created from fahextop/tralrtop records
                {
                    if (fahextop == null && tralrtop == null)
                    {
                        HasErrors = true;
                        return MessageConstant.NewBatchNoResponse;
                    }

                    var isTM = false;
                    var curResponse = fahextop ?? tralrtop;
                    if (curResponse == tralrtop) { trninfop = null; isTM = true; }

                    var curSelector = new RecordTypeSelector(FahSelector);
                    // Instantiate the engine
                    using (var engine = new MultiRecordEngine(curSelector, FahTypes))
                    {
                        // read response file of specified type
                        Results = engine.ReadFile(curResponse);
                        request = CreateRequestFromResults(OpeId, user, isTM, batch.StartDate);
                        var newFile = $"{Path.GetFileNameWithoutExtension(curResponse)}.{request.Id}";
                        engine.WriteFile(Path.Combine(queuePath, newFile), Results);
                    }
                    // optional TRNINFOP processing
                    if (trninfop != null)
                    {
                        // update request receivedon to null for TRNINFOP processing
                        request.ReceivedOn = null;
                        NsldsContext.Entry(request).State = EntityState.Modified;
                        foreach (var student in request.Students)
                        {
                            student.ReceivedOn = null;
                            NsldsContext.Entry(student).State = EntityState.Modified;
                        }
                        // retrieve existing list of student ssn
                        var ssnlist = request.Students.Select(x => x.SSN).ToList();

                        curSelector = new RecordTypeSelector(AckSelector);
                        // Instantiate the engine
                        using (var engine = new MultiRecordEngine(curSelector, AckTypes))
                        {
                            // read response file of specified type
                            Results = engine.ReadFile(trninfop);
                            foreach (var item in Results)
                            {
                                if (item is NsldsAckHeader)
                                {
                                    // sync the new request sequence
                                    var a = item as NsldsAckHeader;
                                    a.Sequence = request.Sequence;
                                }
                                else if (item is NsldsAckRecord)
                                {
                                    var a = item as NsldsAckRecord;
                                    if (!ssnlist.Contains(a.SSN))
                                    {
                                        var student = new ClientRequestStudent
                                        {
                                            Link2ClientRequest = request.Id,
                                            DOB = a.DOB,
                                            FirstName = a.FirstName,
                                            LastName = a.LastName,
                                            RequestType = a.RequestType,
                                            SSN = a.SSN,
                                            StartDate = batch.StartDate,
                                            RevOn = DateTime.Now,
                                            RevBy = user,
                                            SubmittedOn = request.SubmittedOn,
                                            IsSubmitted = true,
                                            ReceivedOn = null,
                                            IsDeleted = false,
                                            IsReceived = false,
                                            IsResolved = false,
                                            IsValid = true,
                                            EnrollBeginDate = batch.StartDate
                                        };
                                        NsldsContext.ClientRequestStudents.Add(student);
                                        ssnlist.Add(a.SSN);
                                    }
                                }
                            }
                            NsldsContext.SaveChanges();

                            var newFile = $"{Path.GetFileNameWithoutExtension(trninfop)}.{request.Id}";
                            engine.WriteFile(Path.Combine(queuePath, newFile), Results);
                            if (HasErrors)
                            {
                                return string.Format(MessageConstant.ResponseErrorFound, Ident.TRNINFOP);
                            }
                        }
                    }

                    if (HasErrors)
                    {
                        return string.Format(MessageConstant.ResponseErrorFound, Ident.FAHEXTOP);
                    }
                }
                // success
                // move pending files to processed folder
                foreach (var file in pendingFiles)
                {
                    File.Move(file, Path.Combine(processedPath, Path.GetFileName(file)));
                }
                // success result message
                var msgResult = (batch.BatchId == 0) ? MessageConstant.NewBatchResponseSuccess : MessageConstant.BatchResponseSuccess;
                batch.BatchId = request.Id;
                return string.Format(msgResult, batch.BatchId);
            }
            catch (Exception ex)
            {
                HasErrors = true;
                return ex.Message;
            }
        }

        #endregion
    }
}