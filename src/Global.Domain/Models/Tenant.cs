using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Global.Domain
{
    [Table("Tenant", Schema = "dbo")]
    public partial class Tenant
    {
        [MaxLength(10)]
        [Key]
        public string TenantId { get; set; }      // This is the OPE_ID
        [MaxLength(255)]
        public string TenantDomain { get; set; }
        [MaxLength(50)]
        public string DatabaseName { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }

        public Tenant()
        {
            CreatedOn = DateTime.Now;
        }
    }
}
