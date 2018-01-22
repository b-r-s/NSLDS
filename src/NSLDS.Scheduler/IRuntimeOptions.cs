using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Global.Domain;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace NSLDS.Scheduler
{
    public interface IRuntimeOptions
    {
        DbContextOptions GetDbContextOptions(ClaimsPrincipal claimsPrincipal, GlobalContext globalContext);
        string GetUserName(ClaimsPrincipal claimsPrincipal);
        string GetTenantId(ClaimsPrincipal claimsPrincipal);
    }
}
