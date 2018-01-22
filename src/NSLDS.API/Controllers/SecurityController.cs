using NSLDS.Domain;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Global.Domain;
using System.Net;
using System.Net.Http;
using NSLDS.Common;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace NSLDS.API.Controllers
{
	/// <summary>
	/// A utility controller with few non-secured endpoints.
	/// </summary>
	//  [Authorize]
	[Produces("application/json")]
	[Route("api/security")]
	public class SecurityController : DbContextController
	{
		public SecurityController(GlobalContext globalContext, IHostingEnvironment hostingEnvironment, IConfiguration configuration, IRuntimeOptions runtimeOptions, ILogger<SecurityController> logger)
				: base(globalContext, hostingEnvironment, configuration, runtimeOptions, logger)
		{

		}

		/// <summary>
		/// Validate tenant code. Primarily used for registration and login process.
		/// </summary>
		/// <param name="tenantCode"></param>
		/// <returns>200 - Ok. Also returns object with "TenantId" string property. To be passed during registration and login.</returns>
		/// <returns>404 - Not found</returns>
		[AllowAnonymous]
		[HttpGet("ValidateTenantCode")]
		public IActionResult ValidateTenantCode([FromQuery] string tenantCode)
		{
			var tenant = GlobalContext.Tenants.Where(t => t.TenantId == tenantCode).SingleOrDefault();
			if (tenant == null)
			{
				return NotFound();
			}
			return Ok(new ValidateTenantResult { TenantId = tenant.TenantId });
		}

		/// <summary>
		/// Validate access token against IdentityServer AccessTokenValidation endpoint.
		/// </summary>
		/// <param name="accessToken"></param>
		/// <returns>200 - Valid</returns>
		/// <returns>403 - Bad Request or expired</returns>
		[AllowAnonymous]
		[HttpGet("ValidateAccessToken")]
		public IActionResult ValidateAccessToken([FromQuery] string accessToken)
		{
			string identityServerUrl = Configuration["IdentityServer:Authority"];

			using (HttpClient httpClient = new HttpClient())
			{
				//try
				//{
				httpClient.BaseAddress = new Uri(identityServerUrl);   // url to web service

				//var response = httpClient.PostAsJsonAsync("/connect/accesstokenvalidation", "token=" + token).Result;
				var response = httpClient.GetAsync("/connect/accesstokenvalidation?token=" + accessToken).Result;
				if (response.StatusCode != HttpStatusCode.OK)
				{
					return BadRequest();
				}

				return Ok();
			}
		}

		[HttpGet("ProductInformation")]
		public IActionResult ProductInformation()
		{
			var versionInfo = NSLDS.API.Startup.GetProductInformation();
			return Json(versionInfo);
		}

		/// <summary>
		/// Validate database connection after login, use init=true to try create the client database
		/// </summary>
		/// <param name="init">true to create the database, false by default</param>
		/// <returns>200 - Valid db connection</returns>
		/// <returns>503 - Service unavailable</returns>
		[HttpGet("CheckDb")]
		public IActionResult CheckDb([FromQuery] bool init = false)
		{
			// apply client DB latest migrations if any
			try
			{
				// if init=true we skip the connection check and create the db
				if (!init)
				{
					// test client database with short connection timeout
					var claims = User.Claims;
					var tenantId = claims.SingleOrDefault(x => x.Type == "TenantId").Value;
					var tenant = GlobalContext.Tenants.Where(t => t.TenantId.ToUpper().Trim() == tenantId.ToUpper().Trim()).SingleOrDefault();
					var conn = string.Format(Configuration["Data:ClientDb:ConnectionString"], tenant.DatabaseName);
					var connBuilder = new SqlConnectionStringBuilder(conn)
					{
						ConnectTimeout = 1
					};
					var optionsbuilder = new DbContextOptionsBuilder();
					optionsbuilder.UseSqlServer(connBuilder.ConnectionString);

					using (var context = new NSLDS_Context(optionsbuilder.Options))
					{
						context.Database.OpenConnection();
					}
				}
				else //if (init)
				{
					NsldsContext.Database.MigrateAsync();
					return new StatusCodeResult((int)HttpStatusCode.Accepted);
				}
				NsldsContext.Database.Migrate();
				return Ok();
			}
			catch (Exception ex)
			{
				return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
			}
		}
	}
}
