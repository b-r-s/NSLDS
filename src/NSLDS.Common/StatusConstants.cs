using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NSLDS.Common
{
    // Summary:
    // Defines all batch request status constants as an enum style static class
    /*
        Validated with Errors
        In Queue
        On Hold
        Retrieval in Progress
        Retrieval Complete
        Partial Retrieval
        Disabled
        //Transfer Student Monitoring Alerts
        Batch Failed
        //Reprocessed  

        datachk-68: TM alert statuses
     */
    public static class StatusConstant
    {
        public const string
            Errors = "Validated with Errors",
            OnHold = "On Hold",
            InQueue = "In Queue",
            InProgress = "Retrieval in Progress",
            Partial = "Partial Retrieval",
            Received = "Retrieval Complete",
            TmNotStarted = "TM Alert not Started",
            TmInProgress = "TM Alert in Progress",
            TmComplete = "TM Alert Complete",
            TmExpired = "TM Alert Expired",
            TmRefresh = "TM Alert Refresh",
            ReceivedErrors = "Received With Errors",
            Failed = "Batch Failed",
            Disabled = "Batch Disabled",
            Deleted = "Request Deleted",
            Expired = "Expired";
    }
}
