using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Global.Domain;
using NSLDS.Common;
using Microsoft.EntityFrameworkCore;

namespace NSLDS.API
{
	public class ProductionRuntimeOptions : AbstractRuntimeOptions
	{
		public ProductionRuntimeOptions(IConfiguration config, DbContextOptionsBuilder dbContextOptionsBuilder)
				: base(config, dbContextOptionsBuilder)
		{ }

		public override DbContextOptions GetDbContextOptions(ClaimsPrincipal claimsPrincipal, GlobalContext globalContext)
		{
			//var claims = claimsPrincipal.Claims;
			//var tenantId = claims.SingleOrDefault(x => x.Type == "TenantId").Value;
			//var tenant = globalContext.Tenants.Where(t => t.TenantId.ToUpper().Trim() == tenantId.ToUpper().Trim()).SingleOrDefault();
			//// tenant not yet created, check userTypes = "datacheck_administrator"
			//if (tenant == null)
			//{
			//	var isAdmin = claims.Any(x => x.Type == "userTypes" && x.Value == "datacheck_administrator");

			//	if (isAdmin)
			//	{
			//		tenant = new Tenant
			//		{
			//			TenantId = tenantId,
			//			CreatedOn = DateTime.Now,
			//			DatabaseName = $"NextGen_{tenantId.Limit(6)}",
			//			IsActive = false,
			//			TenantDomain = $"{tenantId}.globalvfs.com"
			//		};
			//		globalContext.Tenants.Add(tenant);
			//		globalContext.SaveChanges();
			//	}
			//}

			//	var dbName = tenant?.DatabaseName;
			//  Data Source=B-PC\\SQLEXPRESS;Initial Catalog=NextGen_Corp-0
		  var dbName = "NextGen_Corp-0";
			var connectionString = string.Format(this.Configuration["Data:ClientDb:ConnectionString"], dbName);
			this.DbContextOptionsBuilder.UseSqlServer(connectionString);

			return this.DbContextOptionsBuilder.Options;

		}

		public override string GetUserName(ClaimsPrincipal claimsPrincipal)
		{
			//var claims = claimsPrincipal.Claims;
			//var userName = claims.SingleOrDefault(x => x.Type == "preferred_username").Value;

			//return userName;
			return "B_R_S";
		}

		public override string GetTenantId(ClaimsPrincipal claimsPrincipal)
		{
			//var claims = claimsPrincipal.Claims;
			//var tenantId = claims.SingleOrDefault(x => x.Type == "TenantId").Value;

			//return tenantId;
			return "ITTENANT";
		}
	}
}
