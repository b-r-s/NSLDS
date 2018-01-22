using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NSLDS.Common
{
    public static class UploadMethod
    {
        public const string
            TdClient = "T",
            EdConnect = "E",
            TdManual = "M",
            TdGlobal = "G",
            Empty = "";
    }

    public static class RequestType
    {
        public const string
            History = "H",
            Transfer = "T",
            Both = "B";
    }
}
