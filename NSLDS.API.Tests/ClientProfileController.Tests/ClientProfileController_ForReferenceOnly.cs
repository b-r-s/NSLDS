using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

using Castle.MicroKernel;
using Castle.Windsor;
using Castle.Core;
using Moq;
//  using Serilog;

using Global.Domain;
using NSLDS.Domain;
using NSLDS.Common;
using NSLDS.API;
using NSLDS.API.Controllers;
using NSLDS.API.Tests.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq.Language.Flow;
using Moq.Language;
using System.Security.Claims;

namespace NSLDS.API.ClientProfileController.Tests
{
	[TestClass]
	public class ClientProfileController_ForReferenceOnly
	{


		private IWindsorContainer container = null;
		private Mock<DbContextController> dbContext = null;
		private Mock<GlobalContext> globalContext = null;
		private Mock<IHostingEnvironment> host = null;
		private Mock<IConfiguration> config = null;
		private Mock<IRuntimeOptions> opts = null;
		private Mock<ILogger<Controllers.ClientProfileController>> logger = null;
		private Mock<DbContextOptionsBuilder> optionsBuilder = null;
		private Mock<NSLDS_Context> nsldsContext = null;
		private ClientProfile clientProfile = null;
		private List<ClientProfile> clientProfilesList = null;

		private DbSet<ClientProfile> dbClientProfiles = null;


		private ClientProfile[] fakeProfiles = null;


		[TestMethod]
		public void FailWhenNoClientProfileIdIsAvailable()
		{
			// 1. ARRANGE
			//  Create MOCK object(s)
			//  Pass these to the SUT
			logger = new Mock<ILogger<Controllers.ClientProfileController>>();

			//  Global.Domain.GlobalContext
			// 2. ACT
			//  Execute the SUT
			var cpc = new Controllers.ClientProfileController(globalContext.Object, host.Object, config.Object, opts.Object, logger.Object);

			var sut = container.Resolve<Controllers.ClientProfileController>();


			var expected = sut.GetClientProfile() != null;

			// 3. ASSERT
			//  Verify SUT"s intereaction with the MOCK object(s)

		}



		[TestInitialize]
		public void TestInitialize()
		{
			container = new WindsorContainer().Install(new AutoMockInstaller());
			globalContext = new Mock<GlobalContext>();
			globalContext.SetupAllProperties();
			dbContext = new Mock<DbContextController>();
			// public DbContextController(
			// GlobalContext globalContext,// 
			// IHostingEnvironment hostingEnvironment, 
			// IConfiguration configuration,  
			// IRuntimeOptions runtimeOptions, 
			// ILogger<Controller> logger)

			dbContext.SetupAllProperties();
			host = new Mock<IHostingEnvironment>();
			config = new Mock<IConfiguration>();
			opts = new Mock<IRuntimeOptions>();

			var idntity = new System.Security.Principal.GenericIdentity("B_R_S", "NTLM");

			ClaimsIdentity _identity = new ClaimsIdentity(identity: idntity);          //null, null, null, ClaimTypes.Anonymous, ClaimTypes.Anonymous);

			// public ClaimsIdentity(IIdentity identity, 
			//      IEnumerable<Claim> claims, 
			//      string authenticationType, 
			//      string nameType, 
			//      string roleType);
			//  loggers
			var cpcLogger = new Mock<ILogger<Controllers.ClientProfileController>>();
			var acLogger = new Mock<ILogger<AdminController>>();
			var dbLogger = new Mock<ILogger<DbContextController>>();


			CreateClientProfiles();
			fakeProfiles = clientProfilesList.ToArray<ClientProfile>();  //();

			nsldsContext = new Mock<NSLDS_Context>();
			nsldsContext.SetupAllProperties();

			nsldsContext.Setup(cp => cp.ClientProfiles).ReturnsDbSet(fakeProfiles);

			// +		nsldsContext.Object.ClientProfiles.First()	
			// { NSLDS.Domain.ClientProfile}	
			// NSLDS.Domain.ClientProfile




			var adminController = new Mock<AdminController>(globalContext.Object, host.Object, config.Object, opts.Object, acLogger);
			adminController.SetupAllProperties();
			var clientProfileController = new Mock<Controllers.ClientProfileController>(globalContext.Object, host.Object, config.Object, opts.Object, cpcLogger);
			clientProfileController.SetupAllProperties();
			var _dbContextController = new Mock<DbContextController>(globalContext, host, config, opts, dbLogger);
			_dbContextController.SetupAllProperties();



			//  Global.Domain.GlobalContext
			// 2. ACT
			//  Execute the SUT
			//		public ClientProfileController(
			//					GlobalContext globalContext, 
			//					IHostingEnvironment hostingEnvironment, 
			//					IConfiguration configuration,
			//					IRuntimeOptions runtimeOptions, 
			//					ILogger<ClientProfileController> logger) 
			var cpc = new Controllers.ClientProfileController(globalContext.Object, host.Object, config.Object, opts.Object, cpcLogger.Object);

			var sut = container.Resolve<Controllers.ClientProfileController>();


			var expected = sut.GetClientProfile() != null;

			//DbSet<Tenant> tenants = globalContext.Object.Set<Tenant>();

			//globalContext.Object.Tenants = globalContext.Object.Set<Tenant>();
			//Tenant t = new Tenant
			//{
			//	TenantId = "1"
			//};

			//globalContext.Object.Tenants.Add(t);

			//  logger is specific to the controller being tested
			//   initiate in the actual test
			// logger = new Mock<ILogger>();
			optionsBuilder = new Mock<DbContextOptionsBuilder>();

		}
		/*
		 * Id	AY_Definition	Address1	Address2	City	Contact	Email	Exits_Counseling	Expiration	IsDeleted	IsPwdValid	Monitoring	OPEID	Organization_Name	Phone	Retention	RevBy	RevOn	SAIG	State	TD_Password	Zip	Upload_Method
				1	0	105  26th Ave. S.W.	Suite 101	Calgary	BRS_BRS	b@tntzone.com	0	0	0	0	0	ITTENANT	Work@Home	5556781234	365	B_R_S	2018-01-18 20:19:40.163	1234567	Exhausted	NULL	0123	
		 * */
		private void CreateClientProfiles()
		{
			clientProfile = new ClientProfile {
					Id = 1,
					Address1 = "address1",
					Address2 = "address2",
					AY_Definition = 2,
					City = "city",
					Contact = "contact",
					Email = "email",
					Exits_Counseling = false,
					Expiration = 0,
					IsDeleted = false,
					Monitoring = 1,
					OPEID = "ITTENANT",
					Organization_Name = "none",
					Phone = "1234566778",
					Retention = 0,
					RevBy = "BRS",
					RevOn = DateTime.Now,
					SAIG = "1122334455",
					State = "IL",
					Zip = "11987",
					TD_Password = "xyzxyz44444",
					IsPwdValid = true,
					Upload_Method = "",
					LastPwdChanged = null
			};

			clientProfilesList = new List<ClientProfile>
			{
				clientProfile
			};
		}

		[TestCleanup]
		public void TestCleanup()
		{

		}

		//private static DbSet<T> GetQueryableMockDbSet<T>(params T[] sourceList) where T : class
		//{
		//	var queryable = sourceList.AsQueryable();

		//	var dbSet = new Mock<DbSet<T>>();
		//	dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
		//	dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
		//	dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
		//	dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

		//	return dbSet.Object;
		//}
	}

	public static class DbSetMocking
	{
		private static Mock<DbSet<T>> CreateMockSet<T>(IQueryable<T> data)
				where T : class
		{
			var queryableData = data.AsQueryable();
			var mockSet = new Mock<DbSet<T>>();
			mockSet.As<IQueryable<T>>().Setup(m => m.Provider)
					.Returns(queryableData.Provider);
			mockSet.As<IQueryable<T>>().Setup(m => m.Expression)
					.Returns(queryableData.Expression);
			mockSet.As<IQueryable<T>>().Setup(m => m.ElementType)
					.Returns(queryableData.ElementType);
			mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator())
					.Returns(queryableData.GetEnumerator());
			return mockSet;
		}

		public static IReturnsResult<TContext> ReturnsDbSet<TEntity, TContext>(
				this IReturns<TContext, DbSet<TEntity>> setup,
				TEntity[] entities)
			where TEntity : class
			where TContext : DbContext
		{
			var mockSet = CreateMockSet(entities.AsQueryable());
			return setup.Returns(mockSet.Object);
		}

		public static IReturnsResult<TContext> ReturnsDbSet<TEntity, TContext>(
				this IReturns<TContext, DbSet<TEntity>> setup,
				IQueryable<TEntity> entities)
			where TEntity : class
			where TContext : DbContext
		{
			var mockSet = CreateMockSet(entities);
			return setup.Returns(mockSet.Object);
		}

		public static IReturnsResult<TContext> ReturnsDbSet<TEntity, TContext>(
				this IReturns<TContext, DbSet<TEntity>> setup,
				IEnumerable<TEntity> entities)
			where TEntity : class
			where TContext : DbContext
		{
			var mockSet = CreateMockSet(entities.AsQueryable());
			return setup.Returns(mockSet.Object);
		}
	}
}
