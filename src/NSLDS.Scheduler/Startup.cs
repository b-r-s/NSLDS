using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using Global.Domain;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using Quartz.Impl;
using Quartz;

namespace NSLDS.Scheduler
{
    public class Startup
    {
        private string apiName;
        private string apiVersion;
        private string pathToDoc = @"NSLDS.Scheduler.xml";    // This file gets generated/built into the artifacts folder. Copy to project folder prior to deployment, for Swagger to pickup.
        private readonly IHostingEnvironment _hostingEnvironment;
        private IScheduler scheduler;

        public Startup(IHostingEnvironment env)
        {
            _hostingEnvironment = env;

            // Major version number for Swagger/ui
            apiName = Assembly.GetExecutingAssembly().GetName().FullName;
            apiVersion = $"v{Assembly.GetExecutingAssembly().GetName().Version.Major}";

            // build configuration sources
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo
                .RollingFile(Path.Combine(Configuration["TDClientSettings:RootFolder"], "logs", "log-{Date}.txt"),
                    shared:true)
                .WriteTo.Console()
                .CreateLogger();

            #region Job scheduler factory

            // Grab the Scheduler instance from the Factory 
            scheduler = StdSchedulerFactory.GetDefaultScheduler();
            // and start it off
            scheduler.Start();
            // add environment objects
            scheduler.Context.Put("_env", env);
            scheduler.Context.Put("_config", Configuration);
            scheduler.Context.Put("_logger", Log.Logger);

            // define the jobs and tie it to our TDClientJob classes
            IJobDetail sendjob = JobBuilder.Create<TDClientSendJob>()
                .WithIdentity("sendjob", "send")
                .Build();
            IJobDetail receivejob = JobBuilder.Create<TDClientReceiveJob>()
                .WithIdentity("receivejob", "receive")
                .Build();

            // Trigger the jobs to run at the next X minute mark, and then repeat every X minutes
            var sendint = Configuration.GetValue<int>("TDClientSettings:JobSendInterval");
            var receiveint = Configuration.GetValue<int>("TDClientSettings:JobReceiveInterval");
            ITrigger sendtrigger = TriggerBuilder.Create()
                .WithIdentity("sendtrigger", "send")
                .StartAt(DateBuilder.NextGivenMinuteDate(DateTime.Now, sendint))
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(sendint)
                    .RepeatForever())
                .Build();
            ITrigger receivetrigger = TriggerBuilder.Create()
                .WithIdentity("receivetrigger", "receive")
                .StartAt(DateBuilder.NextGivenMinuteDate(DateTime.Now, receiveint))
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(receiveint)
                    .RepeatForever())
                .Build();

            // configure console window & assembly information
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToSt‌​ring();
            Console.Clear();
            Console.Title = $"{assemblyName} ({assemblyVersion}) started at {DateTime.Now}";
            Console.WriteLine($"NSLDS.Scheduler first send job will start at: {sendtrigger.StartTimeUtc}");
            Console.WriteLine($"NSLDS.Scheduler first receive job will start at: {receivetrigger.StartTimeUtc}");

            // Tell quartz to schedule the jobs using our triggers
            scheduler.ScheduleJob(sendjob, sendtrigger);
            scheduler.ScheduleJob(receivejob, receivetrigger);

            #endregion

        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // retrieve GlobalDb connection string
            var connectionString = Configuration["Data:GlobalDb:ConnectionString"];

            // Add CORS services.
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });

            #region Swagger Configuration

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(apiVersion, new Info
                {
                    Version = apiVersion,
                    Title = "DataCheck Web API (National Student Loan Data System Web Scheduler)",
                    Description = "A web scheduler module provided by GlobalFAS Inc.",
                    TermsOfService = "None"
                });
                c.OperationFilter<AuthorizationHeaderParameterOperationFilter>();
                c.DescribeAllEnumsAsStrings();
                c.IncludeXmlComments(pathToDoc);
            });

            //services.ConfigureSwaggerDocument(options =>
            //{
            //    options.SingleApiVersion(new Info
            //    {
            //        Version = "v1",
            //        Title = "National Student Loan Data System Web Scheduler",
            //        Description = "A web scheduler module provided by GlobalFAS Inc.",
            //        TermsOfService = "None"
            //    });
            //    options.OperationFilter(new Swashbuckle.SwaggerGen.XmlComments.ApplyXmlActionComments(pathToDoc));
            //});

            //services.ConfigureSwaggerSchema(options =>
            //{
            //    options.DescribeAllEnumsAsStrings = true;
            //    // don't use this option unless all POCO entities have true properties, not fields
            //    //options.ModelFilter(new Swashbuckle.SwaggerGen.XmlComments.ApplyXmlTypeComments(pathToDoc));
            //});

            #endregion

            services.AddTransient<CustomExceptionFilter>();

            services.AddMvc();

            // define authorization policies based on user claims (AspNetUserClaims table)
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Administrator", policy =>
                    policy.RequireClaim("NSLDS_Role", "Administrator"));
                options.AddPolicy("Editor", policy =>
                    policy.RequireClaim("NSLDS_Role", "Editor", "Administrator"));
                options.AddPolicy("Viewer", policy =>
                    policy.RequireClaim("NSLDS_Role", "Viewer", "Editor", "Administrator"));
                options.AddPolicy("Guest", policy =>
                    policy.RequireClaim("NSLDS_Role", "Guest", "Viewer", "Editor", "Administrator"));
                options.DefaultPolicy = options.GetPolicy("Guest");
            });

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory("AllowAllOrigins"));
            });

            // temporary fix to the self-referential serialization loop in the Json serializer
            //services.Configure<MvcOptions>(option =>
            //{
            //    //Clear all existing output formatters
            //    option.OutputFormatters.Clear();
            //    var jsonOutputFormatter = new JsonOutputFormatter();
            //    //Set ReferenceLoopHandling
            //    jsonOutputFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            //    //Insert above jsonOutputFormatter as the first formatter, you can insert other formatters.
            //    option.OutputFormatters.Insert(0, jsonOutputFormatter);
            //});
            //services.AddMvc();

            //services.AddMvc(opts =>
            //{
            //    opts.Filters.Add(new CustomExceptionFilter());
            //});

            services.AddEntityFrameworkSqlServer()
                .AddDbContext<GlobalContext>(options => options.UseSqlServer(connectionString));

            services.AddTransient<IConfiguration>(_ => Configuration);
            services.AddTransient<DbContextOptionsBuilder>(_ => new DbContextOptionsBuilder());
            services.AddScoped<GlobalContext>();

            services.AddTransient(_ => _hostingEnvironment);

            if (_hostingEnvironment.IsDevelopment())
            {
                // For non-production, where claims are not available
                services.AddScoped<IRuntimeOptions, NoClaimsRuntimeOptions>();
            }
            else
            {
                // For production or where claims are available
                services.AddScoped<IRuntimeOptions, ProductionRuntimeOptions>();
            }

            //services.AddTransient<MailProcessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
        {
            // register OnShutdown event to stop the Quartz scheduler thread
            appLifetime.ApplicationStopping.Register(OnShutdown);

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug(LogLevel.Trace);
            //loggerFactory.AddSerilog();

            //app.UseIISPlatformHandler();

            app.UseStaticFiles();

            app.UseCors("AllowAllOrigins");

            app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions()
            {
                Authority = Configuration["IdentityServer:Authority"],
                ApiName = "NSLDS.API",
                //LegacyAudienceValidation = true,
                // ApiSecret = "8FBD061D-E1D1-400F-9942-DDE0FC2252D3",
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                RequireHttpsMetadata = false
            });

            app.UseMvc();

            #region Swagger Usage

            app.UseSwagger(c =>
            {
                c.RouteTemplate = "swagger/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{apiVersion}/swagger.json", $"{apiName} {apiVersion}");
            });

            #endregion

        }

        private void OnShutdown()
        {
            // shutdown Quartz scheduler
            scheduler.Shutdown();
        }

        // Entry point for the application.
        // public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
