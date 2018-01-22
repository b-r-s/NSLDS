using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NSLDS.Common
{
    // Summary:
    // Defines all route constants as an enum style static class
    public static class RouteConstant
    {
        public const string
            Request = "/requests/{0}",
            Student = "/requests/student/{0}",
            Queue = "/requests/queue/{0}",
            Submit = "/requests/queue/submit/{0}",
            Upload = "/requests/upload",
            Response = "/response/{0}";
    }
}
