using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Moq;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

using Global.Domain;
using NSLDS.Domain;
using NSLDS.Common;
using NSLDS.API;
using NSLDS.API.Controllers;

namespace NSLDS.API.Tests.Common
{
	public class AutoMockInstaller : IWindsorInstaller
	{
		void IWindsorInstaller.Install(
					IWindsorContainer container,
					IConfigurationStore store)
		{
			container.Kernel.Resolver.AddSubResolver(
				new AutoMoqResolver(
						container.Kernel));
			container.Register(Component.For(typeof(Mock<>)));
			container.Register(Classes
				.FromAssemblyContaining<ClientRequestController>()
				.Pick()
				.WithServiceSelf()
				.LifestyleTransient());
			container.Register(Classes
				.FromAssemblyContaining<GlobalContext>()
				.Pick()
				.WithServiceSelf()
				.LifestyleTransient());
			container.Register(Classes
				.FromAssemblyContaining<NoClaimsRuntimeOptions>()
				.Pick()
				.WithServiceSelf()
				.LifestyleTransient());
			container.Register(Classes
				.FromAssemblyContaining<DbContextOptionsBuilder>()
				.Pick()
				.WithServiceSelf()
				.LifestyleTransient());
			container.Register(Classes
				.FromAssemblyContaining<DbContextController>()
				.Pick()
				.WithServiceSelf()
				.LifestyleTransient());
			//	container.Register<GlobalContext>;    NoClaimsRuntimeOptions  DbContextOptionsBuilder
			//.FromAssemblyContaining<Global.Domain.GlobalContext>());
		}
	}
}

