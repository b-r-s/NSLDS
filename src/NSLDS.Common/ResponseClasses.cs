using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileHelpers;

namespace NSLDS.Common
{
    #region Private converter classes

    internal class Nullable0DecimalConverter : ConverterBase
    {
        public override object StringToField(string from)
        {
            if (from == "N/A") { return null; }
            return Convert.ToDecimal(decimal.Parse(from));
        }

        public override string FieldToString(object fieldValue)
        {
            if (fieldValue == null) { return "N/A"; }
            return ((decimal)fieldValue).ToString("#").Replace(".", "");
        }
    }

    internal class Nullable3DecimalConverter : ConverterBase
    {
        public override object StringToField(string from)
        {
            if (from == "000000") { return null; }
            return Convert.ToDecimal(decimal.Parse(from) / 1000);
        }

        public override string FieldToString(object fieldValue)
        {
            if (fieldValue == null) { return "000000"; }
            return ((decimal)fieldValue * 1000).ToString("#").Replace(".", "");
        }
    }

    internal class Nullable4DecimalConverter : ConverterBase
    {
        public override object StringToField(string from)
        {
            if (from == "0000000") { return null; }
            return Convert.ToDecimal(decimal.Parse(from) / 1000);
        }

        public override string FieldToString(object fieldValue)
        {
            if (fieldValue == null) { return "0000000"; }
            return ((decimal)fieldValue * 1000).ToString("#").Replace(".", "");
        }
    }

    internal class Nullable5DecimalConverter : ConverterBase
    {
        public override object StringToField(string from)
        {
            if (from == "0000000") { return null; }
            return Convert.ToDecimal(decimal.Parse(from) / 10000);
        }

        public override string FieldToString(object fieldValue)
        {
            if (fieldValue == null) { return "0000000"; }
            return ((decimal)fieldValue * 10000).ToString("#").Replace(".", "");
        }
    }

    internal class NullableYearConverter : ConverterBase
    {
        public override object StringToField(string from)
        {
            if (from == "N/A") { return null; }
            return from;
        }

        public override string FieldToString(object fieldValue)
        {
            if (fieldValue == null) { return "N/A"; }
            return fieldValue.ToString();
        }
    }

    internal class NullableDateConverter : ConverterBase
    {
        public override object StringToField(string from)
        {
            // "yyyyMMdd"
            if (from == "N/A") { return null; }
            return DateTime.ParseExact(from, "yyyyMMdd", null);
        }

        public override string FieldToString(object fieldValue)
        {
            if (fieldValue == null) { return "N/A"; }
            return ((DateTime)fieldValue).ToString("yyyyMMdd");
        }
    }

    #endregion

    // classes to parse the various response fixed-length records
    // using FileHelpers engine to read and parse the incoming text files

    #region TD Client header/trailer layout

    [FixedLengthRecord(FixedMode.AllowVariableLength)]
    public class TDClientHeader
    {
        [FieldFixedLength(5)]
        public string Ident;

        [FieldFixedLength(7)]
        public string Mailbox;

        [FieldFixedLength(12)]
        [FieldTrim(TrimMode.Right)]
        protected string Unused1;

        [FieldFixedLength(8)]
        public string FileType;

        [FieldFixedLength(9)]
        [FieldTrim(TrimMode.Right)]
        protected string Unused2;

        [FieldFixedLength(17)]
        [FieldConverter(ConverterKind.Int32)]
        public int BatchId;

        [FieldFixedLength(7)]
        [FieldTrim(TrimMode.Right)]
        protected string Unused3;

        [FieldFixedLength(8)]
        public string OpeId;
    }

    [FixedLengthRecord(FixedMode.AllowVariableLength)]
    public class TDClientTrailer
    {
        [FieldFixedLength(5)]
        public string Ident;

        [FieldFixedLength(7)]
        public string Mailbox;

        [FieldFixedLength(12)]
        [FieldTrim(TrimMode.Right)]
        protected string Unused1;

        [FieldFixedLength(8)]
        public string FileType;

        [FieldFixedLength(9)]
        [FieldTrim(TrimMode.Right)]
        protected string Unused2;

        [FieldFixedLength(17)]
        [FieldConverter(ConverterKind.Int32)]
        public int BatchId;

        [FieldFixedLength(7)]
        [FieldTrim(TrimMode.Right)]
        protected string Unused3;

        [FieldFixedLength(8)]
        public string OpeId;
    }

    #endregion

    #region NSLDS acknowledgement records

    [FixedLengthRecord()]
    public class NsldsAckHeader
    {
        [FieldFixedLength(1)]
        public string Ident;

        [FieldFixedLength(46)]
        [FieldTrim(TrimMode.Right)]
        public string Title;

        [FieldFixedLength(8)]
        public string OPEID;

        [FieldFixedLength(8)]
        public string Servicer;

        [FieldFixedLength(8)]
        [FieldConverter(ConverterKind.Date, "yyyyMMdd")]
        public DateTime SubmittedOn;

        [FieldFixedLength(2)]
        [FieldConverter(ConverterKind.Int16)]
        public short Sequence;

        [FieldFixedLength(47)]
        [FieldTrim(TrimMode.Right)]
        protected string Filler;
    }

    [FixedLengthRecord()]
    public class NsldsAckTrailer
    {
        [FieldFixedLength(1)]
        public string Ident;

        [FieldFixedLength(46)]
        [FieldTrim(TrimMode.Right)]
        public string Title;

        [FieldFixedLength(9)]
        [FieldConverter(ConverterKind.Int32)]
        public int RecordCount;

        [FieldFixedLength(9)]
        [FieldConverter(ConverterKind.Int32)]
        public int ErrorCount;

        [FieldFixedLength(9)]
        [FieldConverter(ConverterKind.Int32)]
        public int ErrorRecCount;

        [FieldFixedLength(9)]
        [FieldConverter(ConverterKind.Int32)]
        public int WarningCount;

        [FieldFixedLength(9)]
        [FieldConverter(ConverterKind.Int32)]
        public int WarningRecCount;

        [FieldFixedLength(28)]
        [FieldTrim(TrimMode.Right)]
        protected string Filler;
    }

    [FixedLengthRecord()]
    public class NsldsAckRecord
    {
        [FieldFixedLength(1)]
        public string Ident;

        [FieldFixedLength(9)]
        public string SSN;

        [FieldFixedLength(12)]
        [FieldTrim(TrimMode.Right)]
        public string FirstName;

        [FieldFixedLength(35)]
        [FieldTrim(TrimMode.Right)]
        public string LastName;

        [FieldFixedLength(8)]
        [FieldConverter(ConverterKind.Date, "yyyyMMdd")]
        public DateTime DOB;

        [FieldFixedLength(8)]
        public string OPEID;

        [FieldFixedLength(3)]
        public int FieldInError;

        [FieldFixedLength(3)]
        public int ErrorCode;

        [FieldFixedLength(35)]
        [FieldTrim(TrimMode.Right)]
        public string FieldValue;

        [FieldFixedLength(1)]
        public string RequestType;

        [FieldFixedLength(5)]
        [FieldTrim(TrimMode.Right)]
        protected string Filler;
    }

    #endregion

    #region NSLDS response records

    [FixedLengthRecord()]
    public class NsldsFahHeader
    {
        [FieldFixedLength(1)]
        public string Ident;

        [FieldFixedLength(46)]
        [FieldTrim(TrimMode.Right)]
        public string Title;

        [FieldFixedLength(8)]
        public string OpeId;

        [FieldFixedLength(8)]
        public string Servicer;

        [FieldFixedLength(1)]
        public string ReportType;

        [FieldFixedLength(8)]
        [FieldConverter(ConverterKind.Date, "yyyyMMdd")]
        public DateTime Date;

        [FieldFixedLength(2)]
        [FieldConverter(ConverterKind.Int16)]
        public short Sequence;

        [FieldFixedLength(426)]
        [FieldTrim(TrimMode.Right)]
        protected string Filler;
    }

    [FixedLengthRecord()]
    public class NsldsFahTrailer
    {
        [FieldFixedLength(1)]
        public string Ident;

        [FieldFixedLength(46)]
        [FieldTrim(TrimMode.Right)]
        public string Title;

        [FieldFixedLength(9)]
        [FieldConverter(ConverterKind.Int32)]
        public int RecordCount;

        [FieldFixedLength(444)]
        [FieldTrim(TrimMode.Right)]
        protected string Filler;
    }

    [FixedLengthRecord()]
    public class NsldsFahType1Rec
    {
        [FieldFixedLength(1)]
        public string Ident;

        [FieldFixedLength(9)]
        public string SSN;

        [FieldFixedLength(12)]
        [FieldTrim(TrimMode.Right)]
        public string CurrentFirstName;

        [FieldFixedLength(35)]
        [FieldTrim(TrimMode.Right)]
        public string CurrentLastName;

        [FieldFixedLength(8)]
        [FieldConverter(ConverterKind.Date, "yyyyMMdd")]
        public DateTime CurrentDOB;

        [FieldFixedLength(8)]
        public string OpeId;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool DefaultedLoan;

        [FieldFixedLength(1)]
        public string DischargedLoanCode;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool SatisfactoryRepayLoan;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool ActiveBankruptcy;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? SubPrincipalBal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? UnSubPrincipalBal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? CombinedPrincipalBal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Right)]
        protected string Unused1;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? SubPendingDisb;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? UnSubPendingDisb;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? CombinedPendingDisb;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? SubTotal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? UnSubTotal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? CombinedTotal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Right)]
        protected string Unused2;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? PerkinsTotalPrincipalBal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? PerkinsCurrentAYDisb;

        [FieldFixedLength(1)]
        public string DirectStaffordMPN;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool? PellChange;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool? LoanChange;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool? AggregateChange;

        [FieldFixedLength(12)]
        [FieldTrim(TrimMode.Right)]
        public string FirstName;

        [FieldFixedLength(35)]
        [FieldTrim(TrimMode.Right)]
        public string LastName;

        [FieldFixedLength(8)]
        [FieldConverter(ConverterKind.Date, "yyyyMMdd")]
        public DateTime DOB;

        [FieldFixedLength(1)]
        public string DirectLoanPlusMPN;

        [FieldFixedLength(1)]
        public string UnderGradSubLoanLimit;

        [FieldFixedLength(1)]
        public string UnderGradCombinedLoanLimit;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? UnAllocPrincipalBal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? PlusLoanPrincipalBal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? UnAllocPrincipalTotal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? PlusLoanTotal;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool? ACGChange;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool? SMARTChange;

        [FieldFixedLength(1)]
        public string DirectLoanPlusGradMPN;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool Fraud;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? PlusProPrincipalBal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? PlusProTotal;

        [FieldFixedLength(1)]
        public string GradSubLoanLimit;

        [FieldFixedLength(1)]
        public string GradCombinedLoanLimit;

        [FieldFixedLength(1)]
        protected string Unused3;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool TeachGrantConverted;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? TeachLoanPrincipalBal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? TeachLoanTotal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? UndergradTeachDisbTotal;

        [FieldFixedLength(7)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable5DecimalConverter))]
        public decimal? UndergradEligibilityUsed;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? UndergradRemainingAmount;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? GradTeachTotalDisb;

        [FieldFixedLength(7)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable5DecimalConverter))]
        public decimal? GradEligibilityUsed;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? GradRemainingAmount;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool? TeachGrantDataChange;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool? TeachGrantChange;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool? TeachLoanChange;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? UndergradSubPrincipalBal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? UndergradUnsubPrincipalBal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? UndergradCombinedPrincipalBal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? UndergradUnallocPrincipalBal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? UndergradSubPendingDisb;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? UndergradUnsubPendingDisb;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? UndergradCombinedPendingDisb;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? UndergradSubTotal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? UndergradUnsubTotal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? UndergradCombinedTotal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? UndergradUnallocTotal;

        [FieldFixedLength(4)]
        [FieldTrim(TrimMode.Right)]
        [FieldConverter(typeof(NullableYearConverter))]
        public string UndergradAY;

        [FieldFixedLength(1)]
        [FieldTrim(TrimMode.Right)]
        public string UndergradDependency;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? GradSubPrincipalBal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? GradUnsubPrincipalBal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? GradCombinedPrincipalBal;
 
        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? GradUnallocPrincipalBal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? GradSubPendingDisb;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? GradUnsubPendingDisb;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? GradCombinedPendingDisb;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? GradSubTotal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? GradUnsubTotal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? GradCombinedTotal;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? GradUnallocTotal;

        [FieldFixedLength(4)]
        [FieldTrim(TrimMode.Right)]
        [FieldConverter(typeof(NullableYearConverter))]
        public string GradAY;

        [FieldFixedLength(1)]
        [FieldTrim(TrimMode.Right)]
        public string GradDependency;

        [FieldFixedLength(7)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable4DecimalConverter))]
        public decimal? LifeEligibilityUsed;

        [FieldFixedLength(1)]
        [FieldTrim(TrimMode.Right)]
        public string UnusualEnrollHistory;

        [FieldFixedLength(1)]
        [FieldTrim(TrimMode.Right)]
        public string PellLifeLimit;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool? SulaFlag;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable3DecimalConverter))]
        public decimal? SubUsagePeriod;

        [FieldFixedLength(28)]
        [FieldTrim(TrimMode.Right)]
        protected string Filler;
    }

    [FixedLengthRecord()]
    public class NsldsFahType2Rec
    {
        [FieldFixedLength(1)]
        public string Ident;

        [FieldFixedLength(9)]
        public string SSN;

        [FieldFixedLength(12)]
        [FieldTrim(TrimMode.Right)]
        public string CurrentFirstName;

        [FieldFixedLength(35)]
        [FieldTrim(TrimMode.Right)]
        public string CurrentLastName;

        [FieldFixedLength(8)]
        [FieldConverter(ConverterKind.Date, "yyyyMMdd")]
        public DateTime CurrentDOB;

        [FieldFixedLength(8)]
        public string OpeId;

        [FieldFixedLength(12)]
        [FieldTrim(TrimMode.Right)]
        public string FirstNameHistory;

        [FieldFixedLength(1)]
        [FieldTrim(TrimMode.Right)]
        public string MiddleInitialHistory;

        [FieldFixedLength(35)]
        [FieldTrim(TrimMode.Right)]
        public string LastNameHistory;

        [FieldFixedLength(379)]
        [FieldTrim(TrimMode.Right)]
        protected string Filler;
    }

    [FixedLengthRecord()]
    public class NsldsFahType3Rec
    {
        [FieldFixedLength(1)]
        public string Ident;

        [FieldFixedLength(9)]
        public string SSN;

        [FieldFixedLength(12)]
        [FieldTrim(TrimMode.Right)]
        public string CurrentFirstName;

        [FieldFixedLength(35)]
        [FieldTrim(TrimMode.Right)]
        public string CurrentLastName;

        [FieldFixedLength(8)]
        [FieldConverter(ConverterKind.Date, "yyyyMMdd")]
        public DateTime CurrentDOB;

        [FieldFixedLength(8)]
        public string OpeId;

        [FieldFixedLength(2)]
        public string OverpaymentType;

        [FieldFixedLength(1)]
        public string Overpayment;

        [FieldFixedLength(4)]
        [FieldTrim(TrimMode.Right)]
        public string OverDisbAY;

        [FieldFixedLength(8)]
        [FieldTrim(TrimMode.Right)]
        public string GrantContact;

        [FieldFixedLength(412)]
        [FieldTrim(TrimMode.Right)]
        protected string Filler;
    }

    [FixedLengthRecord()]
    public class NsldsFahType4Rec
    {
        [FieldFixedLength(1)]
        public string Ident;

        [FieldFixedLength(9)]
        public string SSN;

        [FieldFixedLength(12)]
        [FieldTrim(TrimMode.Right)]
        public string CurrentFirstName;

        [FieldFixedLength(35)]
        [FieldTrim(TrimMode.Right)]
        public string CurrentLastName;

        [FieldFixedLength(8)]
        [FieldConverter(ConverterKind.Date, "yyyyMMdd")]
        public DateTime CurrentDOB;

        [FieldFixedLength(8)]
        public string OpeId;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? SchedAmount;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? DisbAmount;

        [FieldFixedLength(6)] // col 86
        protected string Unused1;

        [FieldFixedLength(4)]
        [FieldTrim(TrimMode.Right)]
        public string AwardYear;

        [FieldFixedLength(5)] // col 96
        protected string Unused2;

        [FieldFixedLength(2)] // col 101
        public string TransNumber;

        [FieldFixedLength(8)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(NullableDateConverter))]
        public DateTime? LastDisbDate;

        [FieldFixedLength(3)] // col 111
        [FieldTrim(TrimMode.Right)]
        public string AcceptedVerification;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? FamilyContribution;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? AwardAmount;

        [FieldFixedLength(8)] // col 126
        public string GrantSchoolCode;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool? GrantChangeFlag;

        [FieldFixedLength(8)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(NullableDateConverter))]
        public DateTime? CODPostedDate;

        [FieldFixedLength(7)] // col 143
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable5DecimalConverter))]
        public decimal? EligibilityUsed;

        [FieldFixedLength(2)]
        [FieldTrim(TrimMode.Both)]
        public string GrantType;

        [FieldFixedLength(2)]
        [FieldTrim(TrimMode.Both)]
        public string EligibilityCode;

        [FieldFixedLength(6)] // col 154
        [FieldTrim(TrimMode.Both)]
        public string HSProgramCode;

        [FieldFixedLength(1)] // col 160
        public string AYLevel;

        [FieldFixedLength(21)]
        [FieldTrim(TrimMode.Both)]
        public string AwardId;

        [FieldFixedLength(7)] // col 182
        [FieldTrim(TrimMode.Both)]
        public string CIPCode;

        [FieldFixedLength(4)]
        protected string Unused3;

        [FieldFixedLength(3)] // col 193
        [FieldConverter(ConverterKind.Int16)]
        public short? GrantSequence;

        [FieldFixedLength(7)] // col 196
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable5DecimalConverter))]
        public decimal? TotalEligibilityUsed;

        [FieldFixedLength(1)]
        protected string Unused4;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool? TEACHConverted;

        [FieldFixedLength(8)] // col 205
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(NullableDateConverter))]
        public DateTime? TEACHConversionDate;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool? AddEligibility;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool? Post911DeceasedDep;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool? FirstTimePell;

        [FieldFixedLength(285)] // col 216
        protected string Filler;
    }

    [FixedLengthRecord()]
    public class NsldsFahType5Rec
    {
        [FieldFixedLength(1)]
        public string Ident;

        [FieldFixedLength(9)]
        public string SSN;

        [FieldFixedLength(12)]
        [FieldTrim(TrimMode.Right)]
        public string CurrentFirstName;

        [FieldFixedLength(35)]
        [FieldTrim(TrimMode.Right)]
        public string CurrentLastName;

        [FieldFixedLength(8)]
        [FieldConverter(ConverterKind.Date, "yyyyMMdd")]
        public DateTime CurrentDOB;

        [FieldFixedLength(8)]
        public string OpeId;

        [FieldFixedLength(2)] // col 74
        [FieldTrim(TrimMode.Both)]
        public string LoanTypeCode;

        [FieldFixedLength(2)]
        [FieldTrim(TrimMode.Both)]
        public string LoanStatusCode;

        [FieldFixedLength(8)]
        [FieldConverter(ConverterKind.Date, "yyyyMMdd")]
        public DateTime LoanStatusDate;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? PrincipalBal;

        [FieldFixedLength(8)] // col 92
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(NullableDateConverter))]
        public DateTime? PrincipalDate;

        [FieldFixedLength(8)] // col 100
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(NullableDateConverter))]
        public DateTime? LoanPeriodBeginDate;

        [FieldFixedLength(8)] // col 108
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(NullableDateConverter))]
        public DateTime? LoanPeriodEndDate;

        [FieldFixedLength(3)]
        [FieldTrim(TrimMode.Both)]
        public string CurrentGACode;

        [FieldFixedLength(1)]
        [FieldTrim(TrimMode.Both)]
        public string AcademicLevel;

        [FieldFixedLength(3)] // col 120
        [FieldTrim(TrimMode.Both)]
        public string ContactType;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? NetLoanAmount;

        [FieldFixedLength(8)] // col 129
        [FieldTrim(TrimMode.Both)]
        public string LoanContact;

        [FieldFixedLength(8)] // col 137
        [FieldTrim(TrimMode.Both)]
        public string LoanSchoolCode;

        [FieldFixedLength(1)]
        [FieldTrim(TrimMode.Both)]
        public string AdditionalUnsubLoan;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool? CapitalInterest;

        [FieldFixedLength(8)] // col 147
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(NullableDateConverter))]
        public DateTime? LastLoanDisbDate;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? TotalDisb;

        [FieldFixedLength(6)] // col 161
        [FieldTrim(TrimMode.Both)]
        public string LenderCode;

        [FieldFixedLength(6)] // col 167
        [FieldTrim(TrimMode.Both)]
        public string LenderServicer;

        [FieldFixedLength(1)]
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool? LoanRecChange;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? CalcSubAmount;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? CalcUnsubAmount;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? CalcCombinedAmount;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? UnallocatedAmount;

        [FieldFixedLength(6)] // col 198
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? LastDisbAmount;

        [FieldFixedLength(3)] // col 204
        [FieldTrim(TrimMode.Both)]
        public string PerkinsCancelCode;

        [FieldFixedLength(6)]
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(Nullable0DecimalConverter))]
        public decimal? LoanAmount;

        [FieldFixedLength(8)] // col 213
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(NullableDateConverter))]
        public DateTime? LoanDate;

        [FieldFixedLength(21)] // col 221
        [FieldTrim(TrimMode.Both)]
        public string DataProviderID;

        [FieldFixedLength(1)]
        [FieldTrim(TrimMode.Both)]
        public string ConfirmSubStatus;

        [FieldFixedLength(8)] // col 243
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(NullableDateConverter))]
        public DateTime? ConfirmSubDate;

        [FieldFixedLength(8)] // col 251
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(NullableDateConverter))]
        public DateTime? AYBeginDate;

        [FieldFixedLength(8)] // col 259
        [FieldTrim(TrimMode.Both)]
        [FieldConverter(typeof(NullableDateConverter))]
        public DateTime? AYEndDate;

        [FieldFixedLength(1)] // col 267
        [FieldConverter(ConverterKind.Boolean, "Y", "N")]
        public bool? Reaffirmation;

        [FieldFixedLength(233)] // col 268
        protected string Filler;
    }

    #endregion  
}
