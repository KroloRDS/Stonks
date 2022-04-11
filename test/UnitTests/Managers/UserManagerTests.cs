using System;
using System.Collections.Generic;
using NUnit.Framework;
using Stonks.Helpers;
using Stonks.Managers;

namespace UnitTests.Managers;

[TestFixture]
public class UserManagerTests : ManagerTest
{
	private readonly UserManager _manager;
	private const string CorrectEmail = "test@test.com";

	public UserManagerTests()
	{
		_manager = new UserManager(_ctx);
	}

	[Test]
	public void ChangePayPalEmail_NullUser_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(
			() => _manager.ChangePayPalEmail(null, CorrectEmail));
	}

	[Test]
	public void ChangePayPalEmail_WrongUser_ShouldThrow()
	{
		Assert.Throws<KeyNotFoundException>(
			() => _manager.ChangePayPalEmail(Guid.NewGuid(), CorrectEmail));
	}

	[Test]
	public void ChangePayPalEmail_NullEmail_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(
			() => _manager.ChangePayPalEmail(GetUserId(AddUser()), null));
	}

	[Test]
	public void ChangePayPalEmail_EmptyEmail_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(
			() => _manager.ChangePayPalEmail(GetUserId(AddUser()), string.Empty));
	}

	[Test]
	public void ChangePayPalEmail_WrongEmail_ShouldThrow()
	{
		//Arrange
		var userId = GetUserId(AddUser());
		var wrongEmails = new string[]
		{
			"test",
			"test@",
			"test@test",
			"test@test.",
			"te est@test.com",
			"!?$@test.com",
			"Abc.example.com",
			"A@b@c@example.com",
			"a\"b(c)d,e:f; g<h> i[j\\k]l @example.com",
			"i_like_underscore@but_its_not_allowed_in_this_part.example.com",
			"💩@test.com"
		};
		var longEmail = "1234567890123456789012345678901234567890123456789012345678901234x@example.com";

		//Act & Assert
		foreach (var email in wrongEmails)
		{
			Assert.Throws<InvalidEmailException>(
				() => _manager.ChangePayPalEmail(userId, email));
		}
		Assert.Throws<EmailTooLongException>(
			() => _manager.ChangePayPalEmail(userId, longEmail));
	}

	[Test]
	public void ChangePayPalEmail_PositiveTest()
	{
		var userId = GetUserId(AddUser());
		_manager.ChangePayPalEmail(userId, CorrectEmail);

		var user = _ctx.GetUser(userId);
		Assert.AreEqual(CorrectEmail, user.PayPalEmail);
	}
}
