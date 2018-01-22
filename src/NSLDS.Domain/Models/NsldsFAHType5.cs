using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace NSLDS.Domain
{
    [Table("NsldsFAHType5", Schema = "nslds")]
    public partial class NsldsFAHType5
    {
        public NsldsFAHType5()
        {
        }

        public int Id { get; set; }
        public int Link2ClientRequestStudent { get; set; }
        [MaxLength(2)]
        public string LoanTypeCode { get; set; }
        [MaxLength(2)]
        public string LoanStatusCode { get; set; }
        public DateTime LoanStatusDate { get; set; }
        public decimal? PrincipalBal { get; set; }
        public DateTime? PrincipalDate { get; set; }
        public DateTime? LoanPeriodBeginDate { get; set; }
        public DateTime? LoanPeriodEndDate { get; set; }
        [MaxLength(3)]
        public string CurrentGACode { get; set; }
        [MaxLength(1)]
        public string AcademicLevel { get; set; }
        [MaxLength(3)]
        public string ContactType { get; set; }
        public decimal? NetLoanAmount { get; set; }
        [MaxLength(8)]
        public string LoanContact { get; set; }
        [MaxLength(8)]
        public string LoanSchoolCode { get; set; }
        [MaxLength(1)]
        public string AdditionalUnsubLoan { get; set; }
        public bool? CapitalInterest { get; set; }
        public DateTime? LastLoanDisbDate { get; set; }
        public decimal? TotalDisb { get; set; }
        [MaxLength(6)]
        public string LenderCode { get; set; }
        [MaxLength(6)]
        public string LenderServicer { get; set; }
        public bool? LoanRecChange { get; set; }
        public decimal? CalcSubAmount { get; set; }
        public decimal? CalcUnsubAmount { get; set; }
        public decimal? CalcCombinedAmount { get; set; }
        public decimal? UnallocatedAmount { get; set; }
        public decimal? LastDisbAmount { get; set; }
        [MaxLength(3)]
        public string PerkinsCancelCode { get; set; }
        public decimal? LoanAmount { get; set; }
        public DateTime? LoanDate { get; set; }
        [NotMapped]
        public string DataProviderID { get { return Encryption._decrypt(EncDataProviderID); } set { EncDataProviderID = Encryption._encrypt(value); } }
        [MaxLength(1)]
        public string ConfirmSubStatus { get; set; }
        public DateTime? ConfirmSubDate { get; set; }
        public DateTime? AYBeginDate { get; set; }
        public DateTime? AYEndDate { get; set; }
        public bool? Reaffirmation { get; set; }
        [IgnoreDataMember]
        public string EncDataProviderID { get; set; }

        [IgnoreDataMember]
        [ForeignKey("Link2ClientRequestStudent")]
        public virtual ClientRequestStudent ClientRequestStudent { get; set; }
    }
}
