using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace NSLDS.Domain
{
    [NotMapped]
    public class StudentSearchDetail
    {
        [Key] // non-mapped entities filled from Sql require a key
        public int StudentId { get; set; }
        public int BatchId { get; set; }
        [NotMapped]
        public string SSN { get { return Encryption._decrypt(EncSSN); } }
        [IgnoreDataMember]
        public string EncSSN { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DOB { get; set; }
        public DateTime? RequestDate { get; set; }
        public bool? IsReceived { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string ResponseRoute { get; set; }
        public DateTime? StartDate { get; set; }
    }

}
