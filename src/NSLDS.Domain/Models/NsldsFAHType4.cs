using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace NSLDS.Domain
{
    [Table("NsldsFAHType4", Schema = "nslds")]
    public partial class NsldsFAHType4
    {
        public NsldsFAHType4()
        {
        }

        public int Id { get; set; }
        public int Link2ClientRequestStudent { get; set; }
        public decimal? SchedAmount { get; set; }
        public decimal? DisbAmount { get; set; }
        [MaxLength(4)]
        public string AwardYear { get; set; }
        [MaxLength(2)]
        public string TransNumber { get; set; }
        public DateTime? LastDisbDate { get; set; }
        [MaxLength(3)]
        public string AcceptedVerification { get; set; }
        public decimal? FamilyContribution { get; set; }
        public decimal? AwardAmount { get; set; }
        [MaxLength(8)]
        public string GrantSchoolCode { get; set; }
        public bool? GrantChangeFlag { get; set; }
        public DateTime? CODPostedDate { get; set; }
        public decimal? EligibilityUsed { get; set; }
        [MaxLength(2)]
        public string GrantType { get; set; }
        [MaxLength(2)]
        public string EligibilityCode { get; set; }
        [MaxLength(6)]
        public string HSProgramCode { get; set; }
        [MaxLength(1)]
        public string AYLevel { get; set; }
        [MaxLength(21)]
        public string AwardId { get; set; }
        [MaxLength(7)]
        public string CIPCode { get; set; }
        public short? GrantSequence { get; set; }
        public decimal? TotalEligibilityUsed { get; set; }
        public bool? TEACHConverted { get; set; }
        public DateTime? TEACHConversionDate { get; set; }
        public bool? AddEligibility { get; set; }
        public bool? Post911DeceasedDep { get; set; }
        public bool? FirstTimePell { get; set; }

        [IgnoreDataMember]
        [ForeignKey("Link2ClientRequestStudent")]
        public virtual ClientRequestStudent ClientRequestStudent { get; set; }
    }
}
