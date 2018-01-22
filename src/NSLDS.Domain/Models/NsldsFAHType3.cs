using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace NSLDS.Domain
{
    [Table("NsldsFAHType3", Schema = "nslds")]
    public partial class NsldsFAHType3
    {
        public NsldsFAHType3()
        {
        }

        public int Id { get; set; }
        public int Link2ClientRequestStudent { get; set; }
        [MaxLength(2)]
        public string OverpaymentType { get; set; }
        [MaxLength(1)]
        public string Overpayment { get; set; }
        [MaxLength(4)]
        public string OverDisbAY { get; set; }
        [MaxLength(8)]
        public string GrantContact { get; set; }

        [IgnoreDataMember]
        [ForeignKey("Link2ClientRequestStudent")]
        public virtual ClientRequestStudent ClientRequestStudent { get; set; }
    }
}
