using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

using Castle.MicroKernel;
// env. usings
using System.Data;
using System.Linq;
using Moq.Language.Flow;
using Moq.Language;
using System.Security.Claims;

// 3rd party usings
using Castle.Windsor;
using Castle.Core;
using Moq;


// internal usings
using Global.Domain;
using NSLDS.Domain;
using NSLDS.Common;
using NSLDS.API;
using NSLDS.API.Controllers;
using NSLDS.API.Tests.Common;
using System.Collections.Generic;



namespace NSLDS.API.Tests.ClientProfileControllerTests
{
	[TestClass]
	public class ClientProfileControllerShould
	{
		[TestMethod]
		public void FailWhenNoClientProfileIdIsAvailable()
		{

		}
	}
}
