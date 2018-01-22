using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace NSLDS.Domain
{
    [Table("ClientProfile_History", Schema = "nslds")]
    public partial class ClientProfile_History
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
        [Required]
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public int AY_Definition { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Contact { get; set; }
        [Required]
        public string Email { get; set; }
        public bool Exits_Counseling { get; set; }
        public int Expiration { get; set; }
        public bool IsDeleted { get; set; }
        public int Link2ClientProfile { get; set; }
        public int Monitoring { get; set; }
        [Required]
        [MaxLength(8)]
        public string OPEID { get; set; }
        [Required]
        public string Organization_Name { get; set; }
        [Required]
        public string Phone { get; set; }
        public int Retention { get; set; }
        [Required]
        [MaxLength(450)]
        public string RevBy { get; set; }
        [Required]
        public DateTime? RevOn { get; set; }
        [Required]
        [MaxLength(7)]
        public string SAIG { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string Zip { get; set; }
        public string TD_Password { get; set; }
        public bool IsPwdValid { get; set; }
        [MaxLength(1)]
        public string Upload_Method { get; set; }

        [IgnoreDataMember]
        [ForeignKey("Link2ClientProfile")]
        public virtual ClientProfile ClientProfile { get; set; }
    }
}
