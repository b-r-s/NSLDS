using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NSLDS.Common
{
    /// <summary>
    /// static class defining all sql queries and stored procedures
    /// </summary>
    public static class SqlConstant
    {
        public const string
            StudentSearchSP = "[nslds].[StudentSearch] @cpid={0}, @startdate={1}, @enddate={2}, @ssn={3}, @name1={4}, @name2={5}, @name3={6}, @dob={7}, @openay={8}, @hasfah={9}"
            ;
    }
}
