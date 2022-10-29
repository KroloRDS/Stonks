using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Moq;
using MediatR;
using NUnit.Framework;
using Stonks.Models;
using Stonks.Requests.Commands.Trade.AcceptOffer;

namespace UnitTests.Handlers.Commands.Trade.AcceptOffer;

public class AcceptOfferTest : InMemoryDb
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly AcceptOfferCommandHandler _handler;

    public AcceptOfferTest()
    {
        _handler = new AcceptOfferCommandHandler(_ctx, _mediator.Object);
    }

    [Test]
    public void AcceptOffer_WrongOffer_ShouldThrow()
    {
        var userId = AddUser().Id;
        AssertThrows<KeyNotFoundException>(
            new AcceptOfferCommand(userId, default));
        AssertThrows<KeyNotFoundException>(
            new AcceptOfferCommand(userId, Guid.NewGuid()));
        _mediator.VerifyNoOtherCalls();
    }

    [Test]
    [TestCase(0)]
    [TestCase(-2)]
    [TestCase(-99)]
    public void AcceptOffer_WrongAmount_ShouldThrow(int amount)
    {
        var offer = AddOffer(OfferType.Buy);
        AssertThrows<ArgumentOutOfRangeException>(
            new AcceptOfferCommand(AddUser().Id, offer.Id, amount));
        _mediator.VerifyNoOtherCalls();
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
        var clientId = AddUser().Id;
        var offer = AddOffer(type);
        var offerCopy = CloneOffer(offer);
        if (amount.HasValue)
            Assert.Greater(amount.Value, offer.Amount);

        //Act
        AcceptOffer(new AcceptOfferCommand(clientId, offer.Id));

        //Assert
        VerifyMocks(offerCopy, clientId);
        Assert.False(_ctx.TradeOffer.Any());
    }

    [Test]
    [TestCase(OfferType.Buy)]
    [TestCase(OfferType.Sell)]
    [TestCase(OfferType.PublicOfferring)]
    public void AcceptOffer_NotFullAmount_ShouldNotRemoveOffer(OfferType type)
    {
        //Arrange
        var clientId = AddUser().Id;
        var offer = AddOffer(type);
        var amount = 5;
        var initialAmout = offer.Amount;
        Assert.Greater(initialAmout, amount);

        //Act
        AcceptOffer(new AcceptOfferCommand(clientId, offer.Id, amount));

        //Assert
        VerifyMocks(offer, clientId);
        Assert.AreEqual(1, _ctx.TradeOffer.Count());
        Assert.AreEqual(initialAmout - amount, offer.Amount);
    }

    private void VerifyMocks(TradeOffer offer, Guid userId)
    {
        VerifyMoneyTransfer(offer.Type, offer.Amount * offer.Price,
            userId, offer.WriterId);
        VerifySharesTransfer(offer.Type, offer.Amount, offer.StockId,
            userId, offer.WriterId);
        _mediator.VerifyNoOtherCalls();
    }

    private void VerifyMoneyTransfer(OfferType type,
        decimal offerValue, Guid userId, Guid? offerWriterId)
    {
        if (type == OfferType.Buy)
        {
            if (!offerWriterId.HasValue)
                throw new ArgumentNullException(nameof(offerWriterId));
            VerifyTakeAndGiveMoney(offerValue, offerWriterId.Value, userId);
        }
        else
        {
            VerifyTakeAndGiveMoney(offerValue, userId, offerWriterId);
        }
    }

    private void VerifyTakeAndGiveMoney(decimal amount,
        Guid senderUserId, Guid? recipientUserId = null)
    {
        _mediator.Verify(x => x.Send(It.Is<TakeMoneyCommand>(
            c => c.UserId == senderUserId && c.Amount == amount),
            CancellationToken.None), Times.Once());

        if (!recipientUserId.HasValue) return;
        _mediator.Verify(x => x.Send(It.Is<GiveMoneyCommand>(
            c => c.UserId == recipientUserId.Value && c.Amount == amount),
            CancellationToken.None), Times.Once());
    }

    private void VerifySharesTransfer(OfferType offerType, int amount,
        Guid stockId, Guid userId, Guid? offerWriterId = null)
    {
        _mediator.Verify(x => x.Send(It.Is<TransferSharesCommand>(
            c => c.StockId == stockId &&
            c.Amount == amount &&
            c.BuyerId ==
                (offerType == OfferType.Buy ? offerWriterId : userId) &&
            c.BuyFromUser == (offerType != OfferType.PublicOfferring) &&
            c.SellerId ==
                (offerType == OfferType.Buy ? userId : offerWriterId)),
            CancellationToken.None), Times.Once());
    }

    private void AssertThrows<T>(AcceptOfferCommand command)
        where T : Exception
    {
        Assert.ThrowsAsync<T>(() => _handler.AcceptOffer(
            command, CancellationToken.None));
    }

    private void AcceptOffer(AcceptOfferCommand command)
    {
        _handler.AcceptOffer(command, CancellationToken.None).Wait();
        _ctx.SaveChanges();
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

    [TearDown]
    public void ResetMockCallCounts()
    {
        _mediator.Invocations.Clear();
    }
}
