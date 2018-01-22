using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace NSLDS.Domain
{
    [Table("ClientRequest", Schema = "nslds")]
    public partial class ClientRequest : IClientRequest
	{
        public ClientRequest()
        {
            //Students = new HashSet<ClientRequestStudent>();
            //Alerts = new HashSet<ClientRequestAlert>();
        }

        [Key]
        public int Id { get; set; }
        public int Link2ClientProfile { get; set; }
        public int? Link2Job { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsSubmitted { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public bool IsReceived { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public string Response { get; set; }
        public bool IsFailed { get; set; }
        public short Sequence { get; set; }
        [Required]
        public DateTime? RevOn { get; set; }
        [Required]
        [MaxLength(450)]
        public string RevBy { get; set; }
        public bool IsOnHold { get; set; }
        public bool IsTM { get; set; }
        [NotMapped]
        public bool IsValid;
        [NotMapped]
        public string RouteData;
        [NotMapped]
        public string Status;

        public virtual ICollection<ClientRequestStudent> Students { get; set; }
        public virtual ICollection<ClientRequestAlert> Alerts { get; set; }

        [IgnoreDataMember]
        [ForeignKey("Link2ClientProfile")]
        public virtual ClientProfile ClientProfile { get; set; }
        [IgnoreDataMember]
        [ForeignKey("Link2Job")]
        public virtual Job Job { get; set; }
    }
}
