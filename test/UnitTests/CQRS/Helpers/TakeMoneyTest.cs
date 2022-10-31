using System;
using System.Collections.Generic;
using MediatR;
using NUnit.Framework;
using Stonks.CQRS.Helpers;
using Stonks.Util;
using UnitTests.CQRS.Commands;

namespace UnitTests.CQRS.Helpers;

public class TakeMoneyTest : CommandTest<TakeMoneyCommand>
{
    protected override IRequestHandler<TakeMoneyCommand, Unit> GetHandler()
    {
        return new TakeMoneyCommandHandler(_ctx);
    }

    [Test]
    public void TakeMoney_WrongUser_ShouldThrow()
    {
        AssertThrows<KeyNotFoundException>(
            new TakeMoneyCommand(default, 1M));
        AssertThrows<KeyNotFoundException>(
            new TakeMoneyCommand(Guid.NewGuid(), 1M));
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void TakeMoney_NotPositiveAmount_ShouldThrow(decimal amount)
    {
        AssertThrows<ArgumentOutOfRangeException>(
            new TakeMoneyCommand(AddUser().Id, amount));
    }

    [Test]
    public void TakeMoney_InsufficientFunds_ShouldThrow()
    {
        //Arrange
        var user = AddUser();
        var funds = 5M;
        var toTake = 10M;
        Assert.Less(funds, toTake);

        user.Funds = funds;
        _ctx.SaveChanges();

        //Act & Assert
        AssertThrows<InsufficientFundsException>(
            new TakeMoneyCommand(user.Id, toTake));
    }

    [Test]
    public void TakeMoney_PositiveTest()
    {
        //Arrange
        var user = AddUser();
        var funds = 10M;
        var toTake = 5M;
        Assert.Greater(funds, toTake);

        user.Funds = funds;
        _ctx.SaveChanges();

        //Act
        Handle(new TakeMoneyCommand(user.Id, toTake));

        //Assert
        Assert.AreEqual(funds - toTake, user.Funds);
    }
}
