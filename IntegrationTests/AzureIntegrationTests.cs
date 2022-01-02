using System;
using NUnit.Framework;

namespace IntegrationTests;

public class AzureIntegrationTests
{
	[Test]
	public void ReadAppSettings()
	{
		var actual = Environment.GetEnvironmentVariable("APPSETTING_BATTLEROYALE_FUN");
		Assert.AreEqual("34", actual);
	}
}