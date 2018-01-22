using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Global.Domain
{
    [Table("FahField", Schema = "nslds")]
    public partial class FahField
    {
        [Key]
        [MaxLength(50)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public FahField()
        {
        }
    }

    [Table("FahAlert", Schema = "nslds")]
    public partial class FahAlert
    {
        [Key]
        [MaxLength(50)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public FahAlert()
        {
        }
    }

    [Table("FahCode", Schema = "nslds")]
    public partial class FahCode
    {
        [MaxLength(50)]
        public string FahFieldId { get; set; }
        [MaxLength(10)]
        public string Code { get; set; }
        public string Name { get; set; }

        public FahCode()
        {
        }
    }

    [Table("PellAward", Schema = "nslds")]
    public partial class PellAward
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int AwardYear { get; set; }
        [MaxLength(5)]
        public string AYDisplay { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal AdditionalPercent { get; set; }
    }
}
