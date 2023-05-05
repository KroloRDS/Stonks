using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Moq;
using MediatR;
using NUnit.Framework;

using Stonks.Util;
using Stonks.Data.Models;
using Stonks.CQRS.Helpers;
using Stonks.CQRS.Commands.Trade;

namespace UnitTests.CQRS.Commands.Trade;

public class AcceptOfferTest : CommandTest<AcceptOfferCommand>
{
	private readonly Mock<ITransferShares> _transferShares = new();
	private readonly Mock<IGiveMoney> _giveMoney = new();
	private readonly AcceptOfferCommandHandler _takeMoney;

	public AcceptOfferTest()
	{
		_takeMoney = new AcceptOfferCommandHandler(_ctx,
			_transferShares.Object, _giveMoney.Object);
	}

	protected override IRequestHandler<AcceptOfferCommand, Unit> GetHandler()
	{
		return new AcceptOfferCommandHandler(_ctx,
			_transferShares.Object, _giveMoney.Object);
	}

	[Test]
	[TestCase(default)]
	[TestCase(_zeroGuid)]
	[TestCase(_randomGuid)]
	public void AcceptOffer_WrongOffer_ShouldThrow(Guid id)
    {
        var userId = AddUser().Id;
        AssertThrowsInner<KeyNotFoundException>(
            new AcceptOfferCommand(userId, id));
		_giveMoney.VerifyNoOtherCalls();
		_transferShares.VerifyNoOtherCalls();
    }

    [Test]
    [TestCase(0)]
    [TestCase(-2)]
    [TestCase(-99)]
    public void AcceptOffer_WrongAmount_ShouldThrow(int amount)
    {
        var offer = AddOffer(OfferType.Buy);
		AssertThrowsInner<ArgumentOutOfRangeException>(
            new AcceptOfferCommand(AddUser().Id, offer.Id, amount));
		_giveMoney.VerifyNoOtherCalls();
		_transferShares.VerifyNoOtherCalls();
	}

    [Test]
    [TestCase(OfferType.Buy, null)]
    [TestCase(OfferType.Sell, null)]
    [TestCase(OfferType.PublicOfferring, null)]
    [TestCase(OfferType.Buy, 99)]
    [TestCase(OfferType.Sell, 99)]
    [TestCase(OfferType.PublicOfferring, 99)]
    public void AcceptOffer_FullAmount_ShouldRemoveOffer(
        OfferType type, int? amount)
    {
        //Arrange
        var client = AddUser();
        var offer = AddOffer(type);
		client.Funds = offer.Amount * offer.Price * 2;
		_ctx.SaveChanges();
        var offerCopy = CloneOffer(offer);
        if (amount.HasValue)
            Assert.Greater(amount.Value, offer.Amount);

        //Act
        Handle(new AcceptOfferCommand(client.Id, offer.Id));

        //Assert
        VerifyMocks(offerCopy, client.Id);
        Assert.False(_ctx.TradeOffer.Any());
    }

    [Test]
    [TestCase(OfferType.Buy)]
    [TestCase(OfferType.Sell)]
    [TestCase(OfferType.PublicOfferring)]
    public void AcceptOffer_NotFullAmount_ShouldNotRemoveOffer(OfferType type)
    {
        //Arrange
        var client = AddUser();
        var offer = AddOffer(type);
        var amount = 5;
        var initialAmout = offer.Amount;
		client.Funds = offer.Amount * offer.Price * 2;
		_ctx.SaveChanges();
		Assert.Greater(initialAmout, amount);

		//Act
		Handle(new AcceptOfferCommand(client.Id, offer.Id, amount));

        //Assert
        VerifyMocks(offer, client.Id);
        Assert.AreEqual(1, _ctx.TradeOffer.Count());
        Assert.AreEqual(initialAmout - amount, offer.Amount);
    }

    private void VerifyMocks(TradeOffer offer, Guid userId)
    {
		VerifyGiveMoney(offer.Type, offer.Amount * offer.Price,
            userId, offer.WriterId);
        VerifyTransferShares(offer.Type, offer.Amount, offer.StockId,
            userId, offer.WriterId);
    }

    private void VerifyGiveMoney(OfferType type,
		decimal amount, Guid userId, Guid? offerWriterId)
    {
		if (type != OfferType.PublicOfferring)
		{
			var id = type == OfferType.Buy ? userId : offerWriterId;
			_giveMoney.Verify(x => x.Handle(
				It.Is<Guid>(x => x == id),
				It.Is<decimal>(x => x == amount)), Times.Once());
		}
		_giveMoney.VerifyNoOtherCalls();
    }

    private void VerifyTransferShares(OfferType offerType, int amount,
        Guid stockId, Guid userId, Guid? offerWriterId = null)
    {
        _transferShares.Verify(x => x.Handle(It.Is<TransferSharesCommand>(
            c => c.StockId == stockId &&
            c.Amount == amount &&
            c.BuyerId ==
                (offerType == OfferType.Buy ? offerWriterId : userId) &&
            c.BuyFromUser == (offerType != OfferType.PublicOfferring) &&
            c.SellerId ==
                (offerType == OfferType.Buy ? userId : offerWriterId)),
            CancellationToken.None), Times.Once());
		_transferShares.VerifyNoOtherCalls();
	}

	private static TradeOffer CloneOffer(TradeOffer offer)
    {
        return new TradeOffer
        {
            Amount = offer.Amount,
            Id = offer.Id,
            Price = offer.Price,
            StockId = offer.StockId,
            Type = offer.Type,
            WriterId = offer.WriterId
        };
    }

	[Test]
	[TestCase(default)]
	[TestCase(_zeroGuid)]
	[TestCase(_randomGuid)]
	public void TakeMoney_WrongUser_ShouldThrow(Guid id)
	{
		AssertThrows<KeyNotFoundException>(id, 1M);
	}

	[Test]
	[TestCase(0)]
	[TestCase(-1)]
	public void TakeMoney_NotPositiveAmount_ShouldThrow(decimal amount)
	{
		AssertThrows<ArgumentOutOfRangeException>(AddUser().Id, amount);
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
		AssertThrows<InsufficientFundsException>(user.Id, toTake);
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
		TakeMoney(user.Id, toTake);

		//Assert
		Assert.AreEqual(funds - toTake, user.Funds);
	}

	private void AssertThrows<T>(Guid id, decimal amount)
		where T : Exception
	{
		Assert.ThrowsAsync<T>(() => _takeMoney.TakeMoney(id, amount));
	}

	private void TakeMoney(Guid id, decimal amount)
	{
		_takeMoney.TakeMoney(id, amount).Wait();
		_ctx.SaveChanges();
	}

	[TearDown]
    public void ResetAcceptOfferMocks()
    {
        _giveMoney.Invocations.Clear();
        _transferShares.Invocations.Clear();
    }
}
