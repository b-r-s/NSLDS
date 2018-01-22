using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using NSLDS.Common;
using Global.Domain;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using Quartz.Impl;
using Quartz;

namespace NSLDS.API
{
	public class Startup
	{
		private string apiName;
		private string apiVersion;
		private string pathToDoc = @"NSLDS.API.xml";    // This file gets generated/built into the artifacts folder. Copy to project folder prior to deployment, for Swagger to pickup.
		private readonly IHostingEnvironment _hostingEnvironment;
		private readonly IScheduler _scheduler;

		public Startup(IHostingEnvironment env)
		{
			_hostingEnvironment = env;

			// build configuration sources
			var builder = new ConfigurationBuilder()
					.SetBasePath(env.ContentRootPath)
					.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
					.AddEnvironmentVariables();
			Configuration = builder.Build();

			// configure console window & assembly information in title bar
			var versionInfo = GetProductInformation();
			var assemblyName = versionInfo.Where(x => x.Item1.Equals("Assembly Description")).FirstOrDefault().Item2;
			var assemblyVersion = versionInfo.Where(x => x.Item1.Equals("Assembly Informational Version")).FirstOrDefault().Item2;
			Console.Clear();
			Console.Title = $"{assemblyName} ({assemblyVersion}) started at {DateTime.Now}";

			// Major version number for Swagger/ui
			apiName = Assembly.GetExecutingAssembly().GetName().FullName;
			apiVersion = $"v{Assembly.GetExecutingAssembly().GetName().Version.Major}";

			#region Job Scheduler configuration

			// Grab the Scheduler instance from the Factory 
			_scheduler = StdSchedulerFactory.GetDefaultScheduler();
			// and start it off
			_scheduler.Start();
			// add environment objects
			_scheduler.Context.Put("_env", env);
			_scheduler.Context.Put("_config", Configuration);
			_scheduler.Context.Put("_logger", Log.Logger);

			#endregion
		}

		public IConfigurationRoot Configuration { get; set; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// retrieve GlobalDb connection string
			var connectionString = Configuration["Data:GlobalDb:ConnectionString"];

			// Add CORS services
			var origins = Configuration.GetSection("Cors:Origins").Get<string[]>();
			var methods = Configuration.GetSection("Cors:Methods").Get<string[]>();
			var headers = Configuration.GetSection("Cors:Headers").Get<string[]>();

			services.AddCors(options =>
			{
				options.AddPolicy("AllowedOrigins", builder =>
							{
						builder.WithOrigins(origins)
									.WithMethods(methods)
									.WithHeaders(headers)
									.AllowCredentials();
					});
			});

			#region Swagger Configuration

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc(apiVersion, new Info
				{
					Version = apiVersion,
					Title = Configuration["AppRoles:Title"],
					Description = Configuration["AppRoles:Description"],
					TermsOfService = Configuration["AppRoles:Terms"]
				});
				c.OperationFilter<AuthorizationHeaderParameterOperationFilter>();
				c.DescribeAllEnumsAsStrings();
				c.IncludeXmlComments(pathToDoc);
			});

			#endregion


			services.AddTransient<CustomExceptionFilter>();

			// datachk-212: .net core 1.1 serializes Json in camelCase, revert temporarily
			services.AddMvc();
			//.AddJsonOptions(options =>
			//{
			//    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
			//});

			// define authorization policies based on user claims (AspNetUserClaims table)
			// datachk-196: load policy roles from appsettings.json claims
			services.AddAuthorization(options =>
			{
				AppRole[] roles = Configuration.GetSection("AppRoles:Policies").Get<AppRole[]>();

				foreach (AppRole role in roles)
				{
					options.AddPolicy(role.PolicyName, policy => policy.RequireClaim(role.ClaimType, role.ClaimValues));
				}
			});
			//services.AddAuthorization(options =>
			//{
			//    options.AddPolicy("Administrator", policy =>
			//        policy.RequireClaim("userTypes", "datacheck_administrator"));
			//    options.AddPolicy("Editor", policy =>
			//        policy.RequireClaim("userTypes", "datacheck_editor", "datacheck_administrator"));
			//    options.AddPolicy("Viewer", policy =>
			//        policy.RequireClaim("userTypes", "datacheck_viewer", "datacheck_editor", "datacheck_administrator"));
			//    options.AddPolicy("FileReview", policy =>
			//        policy.RequireClaim("userTypes", "datacheck_fileReview", "datacheck_viewer", "datacheck_editor", "datacheck_administrator"));
			//});

			services.Configure<MvcOptions>(options =>
			{
				options.Filters.Add(new CorsAuthorizationFilterFactory("AllowedOrigins"));
			});

			services.AddEntityFrameworkSqlServer()
					.AddDbContext<GlobalContext>(options => options.UseSqlServer(connectionString));

			services.AddTransient<IConfiguration>(_ => Configuration);
			services.AddTransient(_ => new DbContextOptionsBuilder());
			services.AddScoped<GlobalContext>();

			services.AddTransient(_ => _hostingEnvironment);

			// inject scheduler instance to access from controllers
			services.AddTransient(_ => _scheduler);

			//if (_hostingEnvironment.IsDevelopment())
			//{
			//    // For non-production, where claims are not available
			//    services.AddScoped<IRuntimeOptions, NoClaimsRuntimeOptions>();
			//}
			//else
			//{
			// For production or where claims are available
			services.AddScoped<IRuntimeOptions, ProductionRuntimeOptions>();
			//}

			services.AddTransient<FileImportProcessor>();
			services.AddTransient<FileExportProcessor>();
			services.AddTransient<MailProcessor>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
		{
			// register OnShutdown event to stop the Quartz scheduler thread
			appLifetime.ApplicationStopping.Register(OnShutdown);

			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug(LogLevel.Trace);
			loggerFactory.AddSerilog();

			// app.UseIISPlatformHandler();

			app.UseStaticFiles();

			app.UseCors("AllowedOrigins");

			#region Exception handler to be tested further

			//// application wide exception handler to catch security token exceptions
			app.Use(next => async context =>
			{
				try
				{
					await next(context);
				}
				catch
				{
					if (!context.Response.HasStarted)
					{
						context.Response.StatusCode = 401;
					}
				}
			});

			#endregion

			//JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
			//app.UseJwtBearerAuthentication(new JwtBearerOptions
			//{
			//    Authority = Configuration["IdentityServer:Authority"], // base address of your OIDC server.
			//    Audience = "http://localhost:5001", // base address of your API.
			//    RequireHttpsMetadata = false
			//});

			app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions()
			{
				Authority = Configuration["IdentityServer:Authority"],
				ApiName = "NSLDS.API",
				//LegacyAudienceValidation = true,
				//ApiSecret = "8FBD061D-E1D1-400F-9942-DDE0FC2252D3",
				AutomaticAuthenticate = true,
				AutomaticChallenge = true,
				RequireHttpsMetadata = false
			});

			app.UseMvc();

			#region Swagger Usage
			var s1 = "";
			var s2 = "";

			app.UseSwagger(c =>
			{
				c.RouteTemplate = "swagger/{documentName}/swagger.json";
				s1 = c.RouteTemplate;
			});

			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint($"/swagger/{apiVersion}/swagger.json", $"{apiName} {apiVersion}");
				s2 = $"/swagger/" + apiVersion + $"/swagger.json";

			});

			#endregion

		}

		private void OnShutdown()
		{
			// shutdown Quartz scheduler
			_scheduler.Shutdown();
		}

		public static List<Tuple<string, string>> GetProductInformation()
		{
			// https://github.com/dotnet/sdk/issues/2

			List<Tuple<string, string>> versionInfo = new List<Tuple<string, string>>();

			versionInfo.Add(new Tuple<string, string>("Assembly Title", Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>() == null ? "N/A" : Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title));
			versionInfo.Add(new Tuple<string, string>("Assembly Description", Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>() == null ? "N/A" : Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>().Description));
			versionInfo.Add(new Tuple<string, string>("Assembly Product", Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>() == null ? "N/A" : Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product));
			versionInfo.Add(new Tuple<string, string>("Assembly Informational Version", Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>() == null ? "N/A" : Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion));
			versionInfo.Add(new Tuple<string, string>("Application Version", Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion));
			versionInfo.Add(new Tuple<string, string>("Assembly FileVersion", Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>() == null ? "N/A" : Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version));
			versionInfo.Add(new Tuple<string, string>("Version", Assembly.GetExecutingAssembly().GetName() == null ? "N/A" : Assembly.GetExecutingAssembly().GetName().Version.ToString()));

			return versionInfo;
		}

		// Entry point for the application.
		// public static void Main(string[] args) => WebApplication.Run<Startup>(args);
	}
}
