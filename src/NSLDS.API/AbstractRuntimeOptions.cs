using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Global.Domain;
using Microsoft.EntityFrameworkCore;

namespace NSLDS.API
{
    public abstract class AbstractRuntimeOptions : IRuntimeOptions
    {
        private IConfiguration _config;
        private DbContextOptionsBuilder _dbContextOptionsBuilder = null;

        protected IConfiguration Configuration
        {
            get
            {
                return _config;
            }
        }

        protected DbContextOptionsBuilder DbContextOptionsBuilder
        {
            get
            {
                return _dbContextOptionsBuilder;
            }
        }

        protected AbstractRuntimeOptions()
        {
        }

        public AbstractRuntimeOptions(IConfiguration config, DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            _config = config;
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public abstract DbContextOptions GetDbContextOptions(ClaimsPrincipal claimsPrincipal, GlobalContext globalContext);
        public abstract string GetUserName(ClaimsPrincipal claimsPrincipal);
        public abstract string GetTenantId(ClaimsPrincipal claimsPrincipal);
    }
}
