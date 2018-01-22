using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Global.Domain;
using NSLDS.Domain;
using NSLDS.Common;
using Microsoft.EntityFrameworkCore;

namespace NSLDS.Scheduler
{
    public class ProductionRuntimeOptions : AbstractRuntimeOptions
    {
        public ProductionRuntimeOptions(IConfiguration config, DbContextOptionsBuilder dbContextOptionsBuilder)
            : base(config, dbContextOptionsBuilder)
        { }

        public override DbContextOptions GetDbContextOptions(ClaimsPrincipal claimsPrincipal, GlobalContext globalContext)
        {
            var claims = claimsPrincipal.Claims;
            var tenantId = claims.SingleOrDefault(x => x.Type == "TenantId").Value;
            var tenant = globalContext.Tenants.Where(t => t.TenantId.ToUpper().Trim() == tenantId.ToUpper().Trim()).SingleOrDefault();
            // tenant not yet created, check NSLDS_Role = Administrator
            if (tenant == null)
            {
                var role = claims.SingleOrDefault(x => x.Type == "NSLDS_Role").Value;
                if (role == "Administrator")
                {
                    tenant = new Tenant
                    {
                        TenantId = tenantId,
                        CreatedOn = DateTime.Now,
                        DatabaseName = $"NextGen_{tenantId.Limit(6)}",
                        IsActive = true,
                        TenantDomain = $"{tenantId}.globalvfs.com"
                    };
                    globalContext.Tenants.Add(tenant);
                    globalContext.SaveChanges();

                    // create the client database
                    var conn = string.Format(Configuration["Data:ClientDb:ConnectionString"], tenant.DatabaseName);
                    var optionsbuilder = new DbContextOptionsBuilder();
                    optionsbuilder.UseSqlServer(conn);
                    using (var context = new NSLDS_Context(optionsbuilder.Options))
                    {
                        context.Database.Migrate();
                    }
                }
                else { throw new Exception("Administrator role is required to initialize the database."); }
            }
            var dbName = tenant?.DatabaseName;
            var connectionString = string.Format(this.Configuration["Data:ClientDb:ConnectionString"], dbName);
            this.DbContextOptionsBuilder.UseSqlServer(connectionString);

            return this.DbContextOptionsBuilder.Options;
        }

        public override string GetUserName(ClaimsPrincipal claimsPrincipal)
        {
            var claims = claimsPrincipal.Claims;
            var userName = claims.SingleOrDefault(x => x.Type == "preferred_username").Value;

            return userName;
        }

        public override string GetTenantId(ClaimsPrincipal claimsPrincipal)
        {
            var claims = claimsPrincipal.Claims;
            var tenantId = claims.SingleOrDefault(x => x.Type == "TenantId").Value;

            return tenantId;
        }
    }
}
