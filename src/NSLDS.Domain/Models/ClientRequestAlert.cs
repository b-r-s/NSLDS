using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace NSLDS.Domain
{
    [Table("ClientRequestAlert", Schema = "nslds")]
    public partial class ClientRequestAlert
    {
        [Key]
        public int Id { get; set; }
        public int Link2ClientRequest { get; set; }
        public int? ErrorLevel { get; set; }
        public int? FieldInError { get; set; }
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
        public int? ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public int RecordCount { get; set; }
        public int ErrorCount { get; set; }
        public int ErrorRecCount { get; set; }
        public int WarningCount { get; set; }
        public int WarningRecCount { get; set; }

        [IgnoreDataMember]
        [ForeignKey("Link2ClientRequest")]
        public virtual ClientRequest ClientRequest { get; set; }
    }
}
