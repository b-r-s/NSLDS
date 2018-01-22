using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Global.Domain
{
    [Table("FedSchoolCodeList", Schema = "dbo")]
    public partial class FedSchoolCodeList
    {
        [MaxLength(8)]
        [Key]
        public string SchoolCode { get; set; }
        [MaxLength(255)]
        public string SchoolName { get; set; }
        [MaxLength(255)]
        public string Address { get; set; }
        [MaxLength(255)]
        public string City { get; set; }
        [MaxLength(255)]
        public string StateCode { get; set; }
        [MaxLength(255)]
        public string ZipCode { get; set; }
        [MaxLength(255)]
        public string Province { get; set; }
        [MaxLength(255)]
        public string Country { get; set; }
        [MaxLength(255)]
        public string PostalCode { get; set; }
    }
}
