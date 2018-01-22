using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Global.Domain;
using Microsoft.EntityFrameworkCore;

namespace NSLDS.Scheduler
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
