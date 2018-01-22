﻿using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Global.Domain;
using NSLDS.Common;
using Microsoft.EntityFrameworkCore;

namespace NSLDS.API
{
    public class NoClaimsRuntimeOptions : AbstractRuntimeOptions
    {
        public NoClaimsRuntimeOptions(IConfiguration config, DbContextOptionsBuilder dbContextOptionsBuilder)
            : base(config, dbContextOptionsBuilder)
        { }

        public override DbContextOptions GetDbContextOptions(ClaimsPrincipal claimsPrincipal, GlobalContext globalContext)
        {
            //var dbName = "NextGen_APU";
            //var dbName = "NextGen_Demo";
            var dbName = "NextGen_Corp-0";

            if (claimsPrincipal != null)
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
                            IsActive = false,
                            TenantDomain = $"{tenantId}.globalvfs.com"
                        };
                        globalContext.Tenants.Add(tenant);
                        globalContext.SaveChanges();
                    }
                }
                dbName = tenant?.DatabaseName;
            }
            var connectionString = string.Format(this.Configuration["Data:ClientDb:ConnectionString"], dbName);
            this.DbContextOptionsBuilder.UseSqlServer(connectionString);

            return this.DbContextOptionsBuilder.Options;
        }

        public override string GetUserName(ClaimsPrincipal claimsPrincipal)
        {
            var username = "nsldsUser@globalfas.com";

            if (claimsPrincipal != null)
            {
                var claims = claimsPrincipal.Claims;
                username = claims.SingleOrDefault(x => x.Type == "preferred_username").Value;
            }
            return username;
        }

        public override string GetTenantId(ClaimsPrincipal claimsPrincipal)
        {
            //return "03819300"; // APU
            //return "00000000"; // demo OpeId
            var tenantId = "12345678"; // Dev

            if (claimsPrincipal != null)
            {
                var claims = claimsPrincipal.Claims;
                tenantId = claims.SingleOrDefault(x => x.Type == "TenantId").Value;
            }
            return tenantId;
        }
    }
}
