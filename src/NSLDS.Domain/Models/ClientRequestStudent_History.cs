using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace NSLDS.Domain
{
    [Table("ClientRequestStudent_History", Schema = "nslds")]
    public partial class ClientRequestStudent_History
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Action { get; set; }
        [Required]
        [MaxLength(450)]
        public string ActionBy { get; set; }
        [Required]
        public DateTime? ActionOn { get; set; }
        [MaxLength(1)]
        public string DeleteMonitoring { get; set; }
        public DateTime? DOB { get; set; }
        public DateTime? EnrollBeginDate { get; set; }
        [MaxLength(100)]
        public string FirstName { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsReceived { get; set; }
        public bool IsSubmitted { get; set; }
        public bool IsValid { get; set; }
        [MaxLength(100)]
        public string LastName { get; set; }
        public int Link2ClientRequest { get; set; }
        public int Link2ClientRequestStudent { get; set; }
        public DateTime? MonitorBeginDate { get; set; }
        public DateTime? ReceivedOn { get; set; }
        [MaxLength(1)]
        public string RequestType { get; set; }
        [Required]
        [MaxLength(450)]
        public string RevBy { get; set; }
        [Required]
        public DateTime? RevOn { get; set; }
        [MaxLength(50)]
        public string SID { get; set; }
        [NotMapped]
        public string SSN { get { return Encryption._decrypt(EncSSN); } set { EncSSN = Encryption._encrypt(value); } }
        [IgnoreDataMember]
        [MaxLength(450)]
        public string EncSSN { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public string Response { get; set; }
        public bool IsResolved { get; set; }
        public bool? IsPellReviewed { get; set; }
        public bool? IsLoanReviewed { get; set; }
        public bool? IsTeachReviewed { get; set; }
        public bool? IsGrantReviewed { get; set; }
        public bool IsRefreshed { get; set; }

        [IgnoreDataMember]
        [ForeignKey("Link2ClientRequestStudent")]
        public virtual ClientRequestStudent ClientRequestStudent { get; set; }
    }
}
