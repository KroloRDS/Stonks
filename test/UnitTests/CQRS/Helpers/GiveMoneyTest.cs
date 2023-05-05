using System;
using System.Collections.Generic;
using NUnit.Framework;
using Stonks.CQRS.Helpers;

namespace UnitTests.CQRS.Helpers;

public class GiveMoneyTest : InMemoryDb
{
	private readonly GiveMoney _giveMoney;

	public GiveMoneyTest()
	{
		_giveMoney = new GiveMoney(_ctx);
	}

	[Test]
	[TestCase(default)]
	[TestCase(_zeroGuid)]
	[TestCase(_randomGuid)]
	public void GiveMoney_WrongUser_ShouldThrow(Guid id)
    {
        Assert.ThrowsAsync<KeyNotFoundException>(() =>
			_giveMoney.Handle(id, 1M));
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void GiveMoney_NotPositiveAmount_ShouldThrow(decimal amount)
    {
		Assert.ThrowsAsync<ArgumentOutOfRangeException>(
			() => _giveMoney.Handle(AddUser().Id, amount));
    }

    [Test]
    public void GiveMoney_PositiveTest()
    {
        //Arrange
        var user = AddUser();
        var amount = 1M;

		//Act
		_giveMoney.Handle(user.Id, amount).Wait();
		_ctx.SaveChanges();

        //Assert
        Assert.AreEqual(amount, user.Funds);
    }
}
