using System;
using System.Collections.Generic;
using NUnit.Framework;

using Stonks.Models;
using Stonks.Helpers;
using Stonks.Managers.Trade;

namespace UnitTests.Managers.Trade;

[TestFixture]
public class UserBalanceManagerTests : ManagerTest
{
	private readonly UserBalanceManager _manager;

	public UserBalanceManagerTests()
	{
		_manager = new UserBalanceManager(_ctx);
	}

	[Test]
	public void ChangeBalance_NullUser_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(
			() => _manager.GiveMoney(null, 1M));
		Assert.Throws<ArgumentNullException>(
			() => _manager.TakeMoney(null, 1M));
	}

	[Test]
	public void ChangeBalance_WrongUser_ShouldThrow()
	{
		Assert.Throws<KeyNotFoundException>(
			() => _manager.GiveMoney(Guid.NewGuid(), 1M));
		Assert.Throws<KeyNotFoundException>(
			() => _manager.TakeMoney(Guid.NewGuid(), 1M));
	}

	[Test]
	public void ChangeBalance_NullAmount_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(
			() => _manager.GiveMoney(GetUserId(AddUser()), null));
		Assert.Throws<ArgumentNullException>(
			() => _manager.TakeMoney(GetUserId(AddUser()), null));
	}

	[Test]
	public void ChangeBalance_NegativeAmount_ShouldThrow()
	{
		Assert.Throws<ArgumentOutOfRangeException>(
			() => _manager.GiveMoney(GetUserId(AddUser()), -1M));
		Assert.Throws<ArgumentOutOfRangeException>(
			() => _manager.TakeMoney(GetUserId(AddUser()), -1M));
	}

	[Test]
	public void TakeMoney_InsufficientFunds_ShouldThrow()
	{
		//Arrange
		var amount = 100M;
		var balance = 1M;
		Assert.Positive(amount);
		Assert.Negative(balance - amount);

		var user1 = AddUser();
		var user2 = AddUserWithFunds(balance);

		//Act & Assert
		Assert.Throws<InsufficientFundsException>(
			() => _manager.TakeMoney(GetUserId(user1), amount));

		Assert.Throws<InsufficientFundsException>(
			() => _manager.TakeMoney(GetUserId(user2), amount));
	}

	[Test]
	public void GiveMoney_PositiveAmount_ShouldAddFunds()
	{
		//Arrange
		var amount = 1M;
		var balance = 1M;
		Assert.Positive(amount);
		Assert.Positive(balance);

		var user1 = AddUser();
		var user2 = AddUserWithFunds(balance);

		//Act
		_manager.GiveMoney(GetUserId(user1), amount);
		_manager.GiveMoney(GetUserId(user2), amount);

		//Assert
		Assert.AreEqual(amount, user1.Funds);
		Assert.AreEqual(amount + balance, user2.Funds);
	}

	[Test]
	public void TakeMoney_EnoughMoneyOnUser_ShouldTakeFunds()
	{
		//Arrange
		var amount = 1M;
		var balance = 100M;
		Assert.Positive(amount);
		Assert.Positive(balance - amount);

		var user = AddUserWithFunds(balance);

		//Act & Assert
		_manager.TakeMoney(GetUserId(user), amount);
		Assert.AreEqual(balance - amount, user.Funds);
	}

	[Test]
	public void TransferMoney_NullPayer_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(
			() => _manager.TransferMoney(null, NewUserId(), 1M));
	}

	[Test]
	public void TransferMoney_WrongPayer_ShouldThrow()
	{
		Assert.Throws<KeyNotFoundException>(
			() => _manager.TransferMoney(Guid.NewGuid(), NewUserId(), 1M));
	}

	[Test]
	public void TransferMoney_NullRecipient_ShouldThrow()
	{
		var user = AddUserWithFunds(100M);
		Assert.Throws<ArgumentNullException>(
			() => _manager.TransferMoney(GetUserId(user), null, 1M));
	}

	[Test]
	public void TransferMoney_WrongRecipient_ShouldThrow()
	{
		var user = AddUserWithFunds(100M);
		Assert.Throws<KeyNotFoundException>(
			() => _manager.TransferMoney(GetUserId(user), Guid.NewGuid(), 1M));
	}

	[Test]
	public void TransferMoney_NullAmount_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(
			() => _manager.TransferMoney(NewUserId(), NewUserId(), null));
	}

	[Test]
	public void TransferMoney_WrongAmount_ShouldThrow()
	{
		Assert.Throws<ArgumentOutOfRangeException>(
			() => _manager.TransferMoney(NewUserId(), NewUserId(), -1M));
	}
	
	[Test]
	public void TransferMoney_InsufficientFunds_ShouldThrow()
	{
		Assert.Throws<InsufficientFundsException>(
			() => _manager.TransferMoney(NewUserId(), NewUserId(), 1M));
	}

	[Test]
	public void TransferMoney_PositiveTest()
	{
		//Arrange
		var amount = 100M;
		var recipient = AddUser();
		var payer = AddUserWithFunds(amount);
		Assert.Positive(amount);

		//Act
		_manager.TransferMoney(GetUserId(payer), GetUserId(recipient), amount);

		//Assert
		Assert.Zero(payer.Funds);
		Assert.AreEqual(amount, recipient.Funds);
	}

	private Guid NewUserId()
	{
		return GetUserId(AddUser());
	}

	private User AddUserWithFunds(decimal funds)
	{
		var user = new User { Funds = funds };
		_ctx.Add(user);
		_ctx.SaveChanges();
		return user;
	}
}
