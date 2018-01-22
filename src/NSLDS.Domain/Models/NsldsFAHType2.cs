using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace NSLDS.Domain
{
    [Table("NsldsFAHType2", Schema = "nslds")]
    public partial class NsldsFAHType2
    {
        public NsldsFAHType2()
        {
        }

        public int Id { get; set; }
        public int Link2ClientRequestStudent { get; set; }
        [MaxLength(12)]
        public string CurrentFirstName { get; set; }
        [MaxLength(35)]
        public string CurrentLastName { get; set; }
        public DateTime CurrentDOB { get; set; }
        [MaxLength(12)]
        public string FirstNameHistory { get; set; }
        [MaxLength(1)]
        public string MiddleInitialHistory { get; set; }
        [MaxLength(35)]
        public string LastNameHistory { get; set; }

        [IgnoreDataMember]
        [ForeignKey("Link2ClientRequestStudent")]
        public virtual ClientRequestStudent ClientRequestStudent { get; set; }
    }
}
