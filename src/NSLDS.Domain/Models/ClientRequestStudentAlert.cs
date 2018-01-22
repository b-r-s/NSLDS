using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace NSLDS.Domain
{
    [Table("ClientRequestStudentAlert", Schema = "nslds")]
    public partial class ClientRequestStudentAlert
    {
        public ClientRequestStudentAlert()
        {
        }

        [Key]
        public int Id { get; set; }
        public int Link2ClientRequestStudent { get; set; }
        public int ErrorLevel { get; set; }
        public int FieldInError { get; set; }
        public string FieldName { get; set; }
        [NotMapped]
        public string FieldValue { get { return Encryption._decrypt(EncFieldValue); } set { EncFieldValue = Encryption._encrypt(value); } }
        [IgnoreDataMember]
        public string EncFieldValue { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        [IgnoreDataMember]
        [ForeignKey("Link2ClientRequestStudent")]
        public virtual ClientRequestStudent ClientRequestStudent { get; set; }
    }
}
