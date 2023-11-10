using Moq;
using NUnit.Framework;
using Stonks.Auth.Application.Requests;
using Stonks.Auth.Db;
using Stonks.Auth.Domain.Models;
using Stonks.Auth.Domain.Repositories;
using Stonks.Common.Utils.Models;
using Stonks.Common.Utils.Models.Constants;
using Stonks.Common.Utils.Services;

namespace Stonks.Auth.Tests.Handlers;

[TestFixture]
public class RegisterTest
{
	private readonly Mock<IUserRepository> _user = new();
	private readonly Mock<IDbWriter> _writer = new();
	private readonly Mock<ILogProvider> _log = new();
	private readonly Mock<IAuthService> _auth = new();

	private RegisterHandler _handler
	{
		get => new(_user.Object, _writer.Object, _log.Object, _auth.Object);
	}

	[Test]
	[TestCase(null)]
	[TestCase("")]
	[TestCase(" ")]
	[TestCase("		")]
	public void Register_WrongLogin_ShouldReturnBadRequest(string? login)
	{
		var actual = _handler.Handle(new Register(login, "test")).Result;
		Assert.Multiple(() =>
		{
			Assert.That(actual.Kind, Is.EqualTo(Kind.BadRequest));
			Assert.That(actual.Value, Is.Null);
			Assert.That(actual.Message, Does.StartWith(new ArgumentNullException().Message));
		});
		VerifyNoOtherCalls();
	}

	[Test]
	[TestCase(null)]
	[TestCase("")]
	[TestCase("		")]
	public void Register_WrongPassword_ShouldReturnBadRequest(string? password)
	{
		var actual = _handler.Handle(new Register("test", password)).Result;
		Assert.Multiple(() =>
		{
			Assert.That(actual.Kind, Is.EqualTo(Kind.BadRequest));
			Assert.That(actual.Value, Is.Null);
			Assert.That(actual.Message, Does.StartWith(new ArgumentNullException().Message));
		});
		VerifyNoOtherCalls();
	}

	[Test]
	public void Register_LoginExist_ShouldReturnBadRequest()
	{
		//Arrange
		_user.Setup(x => x.LoginExist(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		//Act
		var actual = _handler.Handle(new Register("test", "test")).Result;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(actual.Kind, Is.EqualTo(Kind.BadRequest));
			Assert.That(actual.Value, Is.Null);
			Assert.That(actual.Message, Is.EqualTo(new LoginAlreadyUsedException().Message));
		});
		_user.Verify(x => x.LoginExist(It.IsAny<string>(),
			It.IsAny<CancellationToken>()), Times.Once());
		VerifyNoOtherCalls();
	}

	[Test]
	public void Register_ValidParameters_ShouldReturnOk()
	{
		//Arrange
		_user.Setup(x => x.LoginExist(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		var token = "token";
		var login = "login";
		_auth.Setup(x => x.CreateAccessToken(It.IsAny<Guid>(), login,
			Enumerable.Empty<string>())).Returns(token);

		//Act
		var actual = _handler.Handle(new Register(login, "test")).Result;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(actual.Kind, Is.EqualTo(Kind.Ok));
			Assert.That(actual.Value, Is.EqualTo(token));
		});
		_user.Verify(x => x.LoginExist(It.IsAny<string>(),
			It.IsAny<CancellationToken>()), Times.Once());
		_user.Verify(x => x.Add(It.IsAny<User>(),
			It.IsAny<CancellationToken>()),Times.Once());
		_writer.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()),
			Times.Once());
		_auth.Verify(x => x.CreateAccessToken(It.IsAny<Guid>(), login,
			Enumerable.Empty<string>()), Times.Once());
		VerifyNoOtherCalls();
	}

	private void VerifyNoOtherCalls()
	{
		_user.VerifyNoOtherCalls();
		_writer.VerifyNoOtherCalls();
		_auth.VerifyNoOtherCalls();
	}

	[TearDown]
	public void ResetMocks()
	{
		_user.Invocations.Clear();
		_writer.Invocations.Clear();
		_auth.Invocations.Clear();

		_user.Reset();
		_writer.Reset();
		_auth.Reset();
	}
}
