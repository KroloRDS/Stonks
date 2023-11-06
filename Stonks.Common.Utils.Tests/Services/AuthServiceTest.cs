using Moq;
using NUnit.Framework;
using Stonks.Common.Utils.Models.Configuration;
using Stonks.Common.Utils.Services;

namespace Stonks.Common.Utils.Tests.Services;

[TestFixture]
public class AuthServiceTest
{
	[Test]
	public void JwtTest()
	{
		//Arrange
		var log = new Mock<ILogProvider>();

		var key = string.Join("", Enumerable.Repeat("test", 35));
		var config = new JwtConfiguration
		{
			Audience = "test.website",
			Issuer = "test.website",
			ExpirationMinutes = 60,
			SigningKey = key
		};

		var service = new AuthService(config, log.Object);

		var expectedId = Guid.NewGuid();
		var login = "test.login";
		var expectedRoles = new[] { "role1", "role2" };

		//Act
		var token = service.CreateAccessToken(expectedId, login, expectedRoles);
		(var actualId, var actualRoles) = service.ReadToken(token);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(token, Is.Not.Null);
			Assert.That(actualId, Is.EqualTo(expectedId));
			Assert.That(actualRoles, Is.EquivalentTo(expectedRoles));
		});
	}
}
