using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace NSLDS.Domain
{
    [Table("Job", Schema = "nslds")]
    public partial class Job
    {
        public Job()
        {
            // ClientRequests = new HashSet<ClientRequest>();
        }

        [Key]
        public int Id { get; set; }
        public DateTime? BilledOn { get; set; }
        public bool IsBilled { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsReceived { get; set; }
        public bool IsSubmitted { get; set; }
        public DateTime JobDate { get; set; }
        public int Link2ClientProfile { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public string Response { get; set; }
        [Required]
        [MaxLength(450)]
        public string RevBy { get; set; }
        [Required]
        public DateTime? RevOn { get; set; }
        public DateTime? SubmittedOn { get; set; }
        [NotMapped]
        public string RouteSubmit { get; set; }

        public virtual ICollection<ClientRequest> ClientRequests { get; set; }

        [IgnoreDataMember]
        [ForeignKey("Link2ClientProfile")]
        public virtual ClientProfile ClientProfile { get; set; }
    }
}
