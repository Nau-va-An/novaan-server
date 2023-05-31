using System;
using Microsoft.Extensions.DependencyInjection;
using NovaanServer.Developer;
using MongoConnector;
using S3Connector;

namespace NovaanServerTest.Developer
{
	public class DevServiceTest
	{
		[Fact]
		public void IsServerHealthy_InputNone_ReturnTrue()
		{
			var devService = new DevService(null, null, null);
			Assert.True(devService.IsServerHealthy());
		}
	}
}

