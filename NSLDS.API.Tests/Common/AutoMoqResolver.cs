using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Moq;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;

namespace NSLDS.API.Tests.Common
{
	public class AutoMoqResolver : ISubDependencyResolver
	{
		private readonly IKernel _kernel;

		public AutoMoqResolver(IKernel kernel)
		{
			_kernel = kernel;
		}

		bool ISubDependencyResolver.CanResolve(CreationContext context, 
				 ISubDependencyResolver contextHandlerResolver, 
				 ComponentModel model, 
				 DependencyModel dependency)
		{
			return dependency.TargetType.IsInterface; ;
		}

		object ISubDependencyResolver.Resolve(CreationContext context, 
					 ISubDependencyResolver contextHandlerResolver, 
					 ComponentModel model, 
					 DependencyModel dependency)
		{
			var mockType = typeof(Mock<>).MakeGenericType(dependency.TargetType);
			return ((Mock)this._kernel.Resolve(mockType)).Object;
		}
	}
}
