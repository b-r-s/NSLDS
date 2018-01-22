using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Global.Domain
{
    [Table("UserInvite", Schema = "nslds")]
    public partial class UserInvite
    {
        [Key]
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required, EmailAddress]
        public string UserEmail { get; set; }
        [Required, StringLength(8)]
        public string OpeId { get; set; }
        public string SenderName { get; set; }
        [Required, EmailAddress]
        public string SenderEmail { get; set; }
        [Required]
        public DateTime? ExpireOn { get; set; }
        [Required]
        public bool HasRegistered { get; set; }
        public string NSLDS_Role { get; set; }
    }
}
