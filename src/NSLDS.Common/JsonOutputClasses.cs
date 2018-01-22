using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Global.Domain;
using NSLDS.Domain;
using System.ComponentModel.DataAnnotations;

namespace NSLDS.Common
{
    #region Request controller output classes

    // 
    public class MessageResult
    {
        public string Message;
    }
    // 
    public class ErrorResult
    {
        public string Message;
    }
    // 
    public class ClientProfilesResult
    {
        public int Count;
        public int Total;
        public int NextId;
        public int PrevId;
        public IEnumerable<ClientProfile> Results { get; set; }
    }
    //
    public class ClientProfileResult
    {
        [Required]
        public ClientProfile Result;
    }
    //
    public class ClientRequestsResult
    {
        public int Count;
        public IEnumerable<ClientRequest> Results { get; set; }
    }
    //
    public class ClientRequestResult
    {
        public bool IsValid;
        public string RouteData;
        public string RouteSubmit;
        [Required]
        public ClientRequest Result { get; set; }
    }
    //
    public class ClientNsldsRequestsResult
    {
        public int BatchId;
        public int Count;
        public bool IsValid;
        public bool IsOnHold;
        public string Status;
        public string RouteData;
        public string RouteSubmit;
        public IEnumerable<ClientRequestStudent> Results { get; set; }
    }
    //
    public class ClientNsldsRequestResult
    {
        public int BatchId;
        public bool IsValid;
        public string Status;
        public string RouteData;
        public string RouteSubmit;
        [Required]
        public ClientRequestStudent Result { get; set; }
    }
    // spreadsheet upload result
    public class FileUploadResult
    {
        public string FileName;
        public int BatchId;
        public bool IsValid;
        public string Status;
        public string RouteData;
        public string Message;
    }
    // all files upload result
    public class FilesUploadResult
    {
        public int Count;
        public HashSet<FileUploadResult> Results { get; private set; }
        public FilesUploadResult()
        {
            Results = new HashSet<FileUploadResult>();
        }
    }
    // batch inform file download
    public class InformFileResult
    {
        public int BatchId;
        public short Sequence;
        public string Status;
        public string RouteData;
        public string Message;
    }
    // ResponseBatch details
    public class ResponseBatchResult
    {
        public string UploadId;
        public string UploadDate;
        public string FileName;
        public int BatchId;
        public DateTime SubmittedOn;
        public int Sequence;
        public int RecordCount;
        public DateTime StartDate;
        public bool Ignore = false;
    }
    // response files uploaded
    public class ResponseFilesResult
    {
        public string UploadId;
        public string UploadDate;
        public int FileCount;
        public List<ClientRequest> ValidRequests { get; set; }
        public List<ResponseBatchResult> Results { get; set; }
        public ResponseFilesResult()
        {
            ValidRequests = new List<ClientRequest>();
            Results = new List<ResponseBatchResult>();
        }
    }
    //
    public class ClientJobResult
    {
        [Required]
        public Job Result { get; set; }
    }
    //
    public class ClientJobsResult
    {
        public int Count;
        public IEnumerable<Job> Results { get; set; }
    }

    public struct StudentSearchResult
    {
        public int Count;
        public List<StudentSearchDetail> Results;
    }
    //
    public class FederalStudentCodesResult
    {
        public int Count;
        public int Total;
        public string NextId;
        public string PrevId;
        public IEnumerable<FedSchoolCodeList> Results { get; set; }
    }
    //
    public class FederalStudentCodeResult
    {
        [Required]
        public FedSchoolCodeList Result;
    }

    #endregion

    #region Response controller output classes & struct

    // SummaryResult sub-structs
    public struct Field
    {
        public string Display;
        public string Description;
        public string Value;
    }

    public struct NameHistory
    {
        public string Display;
        public string Description;
        public bool Changed;
        public Field FirstName;
        public Field LastName;
    }

    public struct SULA
    {
        public string Display;
        public string Description;
        public Field Flag;
        public Field Usage;
    }

    public struct Status
    {
        public string Display;
        public string Description;
        public Field Independent;
        public Field Dependent;
        public Field OutstandingPrincipalBal;
        public Field PendingDisbursement;
    }

    public struct LoanUndergrad
    {
        public string Display;
        public string Description;
        public Status Unsubsidized;
        public Status Subsidized;
    }

    public struct LoanGrad
    {
        public string Display;
        public string Description;
        public Field Subsidized;
        public Field SubOutstandingPrincipalBal;
        public Field SubPendingDisbursement;
        public Field Unsubsidized;
        public Field OutstandingPrincipalBal;
        public Field PendingDisbursement;
    }

    // datachk-187: additional eligibility per award year
    public class AdditionalEligibility
    {
        public string AYear;
        public bool Apply;
        public decimal? TentativeAmount;
        public decimal? RevisedAmount;
        public string EligiblePercent;
        public string EligibleAmount;
    }

    public struct AwardYear
    {
        public string AYear;
        public decimal? TentativeAmount;
        public decimal? RevisedAmount;
        public string EligiblePercent;
        public string EligibleAmount;
        public AdditionalEligibility AdditionalEligibility;
    }

    public struct Awards
    {
        public string Display;
        public string Description;
        public List<AwardYear> Values;
    }

    public struct Grant
    {
        public string Display;
        public string Description;
        public Field PellLEU;
        public Awards AnnualAwards;
    }

    // Pell awards recalc result struct
    public struct AwardsRecalcResult
    {
        public List<AwardYear> Values;
    }

    // main NsldsSummaryResult struct
    public struct NsldsSummaryResult
    {
        public Field ValidUntil;
        public NameHistory NameHistory;
        public Field UnusualEnrollment;
        public Field Alerts;
        public bool IsResolved;
        public Field OpenAY;
        public SULA SULA;
        public LoanUndergrad UndergraduateLoans;
        public LoanGrad GraduateLoans;
        public Grant Grants;
        public Field OtherFunding;
    }

    // Nslds detail result sub struct
    public struct HeaderField
    {
        public string Display;
        public string Description;
    }

    public struct NameHistoryRow
    {
        public string FirstName;
        public string MiddleInitial;
        public string LastName;
    }

    public struct NameHistoryDetail
    {
        public string Display;
        public string Description;
        public bool Changed;
        public List<HeaderField> HeaderRow;
        public List<NameHistoryRow> DataRows;
    }

    public struct UndergradLevelRow
    {
        public string GradeLevel;
        public string Subsidized;
        public string Unsubsidized;
    }

    public struct GradLevelRow
    {
        public string GradeLevel;
        public string Unsubsidized;
    }

    public struct UndergradLevelEligibility
    {
        public string Display;
        public string Description;
        public List<HeaderField> HeaderRow;
        public List<UndergradLevelRow> DataRows;
    }

    public struct GradLevelEligibility
    {
        public string Display;
        public string Description;
        public List<HeaderField> HeaderRow;
        public List<GradLevelRow> DataRows;
    }

    public struct UndergradLoanRemain
    {
        public string Display;
        public string Description;
        public Field TotalRemain;
        public Field SubLoanRemain;
        public Field UnsubLoanRemain;
        public UndergradLevelEligibility GradeLevel;
    }

    public struct GradLoanRemain
    {
        public string Display;
        public string Description;
        public Field TotalRemain;
        public Field UnsubLoanRemain;
        public GradLevelEligibility GradeLevel;
    }

    public struct LoanAggregateLimit
    {
        public string Display;
        public string Description;
        public UndergradLoanRemain UndergradDependent;
        public UndergradLoanRemain UndergradIndependent;
        public GradLoanRemain Graduate;
        public LoanHistory LoanHistory;
    }

    public struct OpenLoanDetailRow
    {
        public string LoanType;
        public string ApprovedAmount;
        public string PendingDisbAmount;
        public string DisbursedAmount;
        public string PeriodDates;
        public string AYDates;
        public string Servicer;
        public string School;
        public bool? TmFlag;
    }

    public struct OpenLoanDetail
    {
        public List<HeaderField> HeaderRow;
        public List<OpenLoanDetailRow> DataRows;
    }

    public struct OpenAcademicYear
    {
        public string Display;
        public string Description;
        public string OpenAY;
        public OpenLoanDetail OpenLoanDetails;
        public UndergradLevelEligibility OpenUndergradDependent;
        public UndergradLevelEligibility OpenUndergradIndependent;
        public GradLevelEligibility OpenGraduate;
    }

    public struct SubUsageLimit
    {
        public string Display;
        public string Description;
        public string SULA;
        public Field MEP;
        public Field SubUsage;
        public Field REP;
    }

    public struct PellDetailRow
    {
        public string AwardYear;
        public string AwardAmount;
        public string DisbursedAmount;
        public string AYPercentUsed;
        public string EFC;
        public string School;
        public bool? TmFlag;
    }

    public struct PellHistory
    {
        public string Display;
        public string Description;
        public List<HeaderField> HeaderRow;
        public List<PellDetailRow> DataRows;
    }

    public struct Pell
    {
        public string Display;
        public string Description;
        public Field LEU;
        public Field Remain;
        public Awards AnnualRemain;
        public PellHistory PellHistory;
    }

    public struct TeachDetailRow
    {
        public string AwardYear;
        public string AwardAmount;
        public string DisbursedAmount;
        public string AYLevel;
        public string School;
    }

    public struct TeachHistory
    {
        public string Display;
        public string Description;
        public List<HeaderField> HeaderRow;
        public List<TeachDetailRow> DataRows;
    }

    public struct Teach
    {
        public string Display;
        public string Description;
        public Field UndergradReceived;
        public Field UndergradRemain;
        public Field GradReceived;
        public Field GradRemain;
        public TeachHistory TeachHistory;
    }

    public struct ACGDetailRow
    {
        public string AwardYear;
        public string AwardAmount;
        public string DisbursedAmount;
        public string HSProgram;
        public string AYPercentUsed;
        public string School;
    }

    public struct ACGHistory
    {
        public string Display;
        public string Description;
        public List<HeaderField> HeaderRow;
        public List<ACGDetailRow> DataRows;
    }

    public struct ACG
    {
        public string Display;
        public string Description;
        public Field TotalAwarded;
        public Field TotalReceived;
        public Field TotalPercentUsed;
        public ACGHistory ACGHistory;
    }

    public struct SmartDetailRow
    {
        public string AwardYear;
        public string AwardAmount;
        public string DisbursedAmount;
        public string CIPCode;
        public string AYPercentUsed;
        public string School;
    }

    public struct SmartHistory
    {
        public string Display;
        public string Description;
        public List<HeaderField> HeaderRow;
        public List<SmartDetailRow> DataRows;
    }

    public struct Smart
    {
        public string Display;
        public string Description;
        public Field TotalAwarded;
        public Field TotalReceived;
        public Field TotalPercentUsed;
        public SmartHistory SmartHistory;
    }

    public struct GrantHistory
    {
        public string Display;
        public string Description;
        public Pell Pell;
        public Teach Teach;
        public ACG ACG;
        public Smart Smart;
    }

    public struct LoanDetailRow
    {
        public string LoanType;
        public string LoanStatus;
        public string LoanFlag;
        public string LoanStatusDate;
        public string LoanAmount;
        public string DisbursedAmount;
        public string LoanDates;
        public string AYDates;
        public string AcademicLevel;
        public string School;
        public bool? TmFlag;
    }

    public struct LoanHistory
    {
        public string Display;
        public string Description;
        public List<HeaderField> HeaderRow;
        public List<LoanDetailRow> DataRows;
    }

    public struct Loans
    {
        public string Display;
        public string Description;
        public Field ParentPlusBalance;
        public Field GraduatePlusBalance;
        public Field PerkinsBalance;
        public Field PerkinsAYDisbursed;
        public Field TeachBalance;
        public LoanHistory TotalLoanHistory;
    }

    // Nslds detail result main struct
    public struct NsldsDetailResult
    {
        public NameHistoryDetail NameHistory;
        public LoanAggregateLimit LoanAggregateLimits;
        public OpenAcademicYear OpenAcademicYear;
        public SubUsageLimit SULA;
        public bool OtherFunding;
        public GrantHistory Grants;
        public Loans Loans;
    }

    #endregion

    #region Response export classes & structures

    public class LoanExportRow
    {
        public string SSN;
        public string FirstName;
        public string LastName;
        public string LoanType;
        public string LoanStatus;
        public string LoanFlag;
        public DateTime? LoanStatusDate;
        public decimal? LoanAmount;
        public decimal? DisbursedAmount;
        public string LoanDates;
        public string AYDates;
        public string AcademicLevel;
        public string School;
        public string OpenAY;
        public decimal? TotalRemain;
    }

    #endregion
}
