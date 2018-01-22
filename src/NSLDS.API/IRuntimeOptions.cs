using Global.Domain;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace NSLDS.API
{
    public interface IRuntimeOptions
    {
        DbContextOptions GetDbContextOptions(ClaimsPrincipal claimsPrincipal, GlobalContext globalContext);
        string GetUserName(ClaimsPrincipal claimsPrincipal);
        string GetTenantId(ClaimsPrincipal claimsPrincipal);
    }
}
