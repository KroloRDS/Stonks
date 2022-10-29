using System;
using System.Collections.Generic;
using MediatR;
using NUnit.Framework;
using Stonks.Requests.Commands.Trade.AcceptOffer;

namespace UnitTests.Handlers.Commands.Trade.AcceptOffer;

public class GiveMoneyTest : CommandTest<GiveMoneyCommand>
{
    protected override IRequestHandler<GiveMoneyCommand, Unit> GetHandler()
    {
        return new GiveMoneyCommandHandler(_ctx);
    }

    [Test]
    public void GiveMoney_WrongUser_ShouldThrow()
    {
        AssertThrows<KeyNotFoundException>(
            new GiveMoneyCommand(default, 1M));
        AssertThrows<KeyNotFoundException>(
            new GiveMoneyCommand(Guid.NewGuid(), 1M));
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void GiveMoney_NotPositiveAmount_ShouldThrow(decimal amount)
    {
        AssertThrows<ArgumentOutOfRangeException>(
            new GiveMoneyCommand(AddUser().Id, amount));
    }

    [Test]
    public void GiveMoney_PositiveTest()
    {
        //Arrange
        var user = AddUser();
        var amount = 1M;

        //Act
        Handle(new GiveMoneyCommand(user.Id, amount));

        //Assert
        Assert.AreEqual(amount, user.Funds);
    }
}
