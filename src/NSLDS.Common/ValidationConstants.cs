using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace NSLDS.Common
{
    // Summary:
    // Defines all validation message constants as an enum style static class
    public static class NsldsValidator
    {
        public static class Field
        {
            public const string
            SSN = "SSN",
            First = "FirstName",
            Last = "LastName",
            DOB = "DOB",
            RequestType = "RequestType",
            EnrollBegin = "EnrollBeginDate",
            MonitorBegin = "MonitorBeginDate",
            StartDate = "StartDate",
            DelMonitor = "DeleteMonitoring"
            ;
        }

        public static class Message
        {
            public const string
                SSN = "SSN must be 9 digits",
                First = "Student's first name cannot be blank; use NFN if no first name",
                Last = "Student's last name cannot be blank; use NLN if no last name",
                DOB = "Student must be older than 12 and younger than 100",
                RequestType = "Only T, H or B are valid",
                EnrollBegin = "Enrollment date must be between {0} and {1}",
                MonitorBegin = "Monitor begin date cannot be blank or greater than enrollment date",
                StartDate = "Start date cannot be blank",
                DelMonitor = "Y or N for transfer monitoring or blank for financial aid history",
                FAHNoDates = "Enrollment and monitoring begin dates have been removed because the request is for financial aid history"
                ;
        }

        //nslds-147: bug fix for enroll begin date validation message
        //private static DateTime aDate { get { return DateTime.Today; } }

        public static readonly string[]
            SSN = { Field.SSN, Message.SSN },
            First = { Field.First, Message.First },
            Last = { Field.Last, Message.Last },
            DOB = { Field.DOB, Message.DOB },
            RequestType = { Field.RequestType, Message.RequestType },
            EnrollBegin = { Field.EnrollBegin, Message.EnrollBegin },
            MonitorBegin = { Field.MonitorBegin, Message.MonitorBegin },
            StartDate = { Field.StartDate, Message.StartDate },
            DelMonitor = { Field.DelMonitor, Message.DelMonitor },
            FAHNoDates = { Field.RequestType, Message.FAHNoDates }
            ;
    }

    public static class RegExConstant
    {
        public const string
            SSN = @"^[0-9]{9}$",
            RequestType = @"^[T,H,B]{1}$",
            FAH = @"^[H]{1}$",
            NotFAH = @"^[T,B]{1}$",
            Delete = @"^[Y,N]{1}$",
            NoDelete = @"^[ ]{1}$";
    }
}
