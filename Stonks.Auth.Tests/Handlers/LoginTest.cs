using Moq;
using NUnit.Framework;
using Stonks.Auth.Application.Requests;
using Stonks.Auth.Domain.Models;
using Stonks.Auth.Domain.Repositories;
using Stonks.Common.Utils.Models;
using Stonks.Common.Utils.Services;

namespace Stonks.Auth.Tests.Handlers;

[TestFixture]
public class LoginTest
{
	private readonly Mock<IUserRepository> _user = new();
	private readonly Mock<ILogProvider> _log = new();
	private readonly Mock<IAuthService> _auth = new();

	private LoginHandler _handler
	{
		get => new(_user.Object, _log.Object, _auth.Object);
	}

	[Test]
	public void Login_LoginDoesNotExist_ShouldReturnBadRequest()
	{
		//Arrange
		var request = new Login("test", "test");
		_user.Setup(x => x.LoginExist(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		//Act
		var actual = _handler.Handle(request).Result;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(actual.Kind, Is.EqualTo(Kind.BadRequest));
			Assert.That(actual.Value, Is.Null);
			Assert.That(actual.Message, Is.EqualTo("User does not exist"));
		});
		_user.Verify(x => x.LoginExist(It.IsAny<string>(),
			It.IsAny<CancellationToken>()), Times.Once());
		VerifyNoOtherCalls();
	}

	[Test]
	public void Login_WrongPassword_ShouldReturnBadRequest()
	{
		//Arrange
		var request = new Login("test", "test");
		_user.Setup(x => x.LoginExist(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		short salt = 5;
		var hash = AuthService.Hash("password", salt);
		_user.Setup(x => x.GetUserFromLogin(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new User
			{
				PasswordHash = hash,
				Salt = salt
			});

		//Act
		var actual = _handler.Handle(request).Result;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(actual.Kind, Is.EqualTo(Kind.BadRequest));
			Assert.That(actual.Value, Is.Null);
			Assert.That(actual.Message, Is.EqualTo("Wrong password"));
		});
		_user.Verify(x => x.LoginExist(It.IsAny<string>(),
			It.IsAny<CancellationToken>()), Times.Once());
		_user.Verify(x => x.GetUserFromLogin(It.IsAny<string>(),
			It.IsAny<CancellationToken>()), Times.Once());
		VerifyNoOtherCalls();
	}

	[Test]
	public void Login_CorrectLoginAndPassword_ShouldReturnOk()
	{
		//Arrange
		var login = "login";
		var password = "password";
		var request = new Login(login, password);
		_user.Setup(x => x.LoginExist(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		var id = Guid.NewGuid();
		short salt = 5;
		var hash = AuthService.Hash(password, salt);
		_user.Setup(x => x.GetUserFromLogin(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new User
			{
				Id = id,
				Login = login,
				PasswordHash = hash,
				Roles = Enumerable.Empty<Role>(),
				Salt = salt,
			});

		var token = "token";
		_auth.Setup(x => x.CreateAccessToken(id, login,
			Enumerable.Empty<string>())).Returns(token);

		//Act
		var actual = _handler.Handle(request).Result;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(actual.Kind, Is.EqualTo(Kind.Ok));
			Assert.That(actual.Value, Is.EqualTo(token));
		});
		
		_user.Verify(x => x.LoginExist(It.IsAny<string>(),
			It.IsAny<CancellationToken>()), Times.Once());
		_user.Verify(x => x.GetUserFromLogin(It.IsAny<string>(),
			It.IsAny<CancellationToken>()), Times.Once());
		_auth.Verify(x => x.CreateAccessToken(id, login,
			Enumerable.Empty<string>()), Times.Once());
		VerifyNoOtherCalls();
	}

	private void VerifyNoOtherCalls()
	{
		_user.VerifyNoOtherCalls();
		_auth.VerifyNoOtherCalls();
	}

	[TearDown]
	public void ResetMocks()
	{
		_user.Invocations.Clear();
		_auth.Invocations.Clear();

		_user.Reset();
		_auth.Reset();
	}
}
