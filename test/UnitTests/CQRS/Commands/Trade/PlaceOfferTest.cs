using MediatR;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using Moq;
using System.Threading;
using Stonks.Data.Models;
using Stonks.Util;
using Stonks.CQRS.Commands.Trade;

namespace UnitTests.CQRS.Commands.Trade;

public class PlaceOfferTest : CommandTest<PlaceOfferCommand>
{
    protected override IRequestHandler<PlaceOfferCommand, Unit> GetHandler()
    {
        return new PlaceOfferCommandHandler(_ctx, _mediator.Object);
    }

    [Test]
    [TestCase(default)]
	[TestCase(_zeroGuid)]
	[TestCase(_randomGuid)]
	public void PlaceOffer_WrongStock_ShouldThrow(Guid id)
    {
        AssertThrows<KeyNotFoundException>(new PlaceOfferCommand(
			id, AddUser().Id, 1, OfferType.Buy, 1M));
        _mediator.VerifyNoOtherCalls();
    }

    [Test]
    public void PlaceOffer_BankruptStock_ShouldThrow()
    {
        var command = new PlaceOfferCommand(AddBankruptStock().Id,
            AddUser().Id, 1, OfferType.Buy, 1M);
        AssertThrows<BankruptStockException>(command);
        _mediator.VerifyNoOtherCalls();
    }

	[Test]
	[TestCase(default)]
	[TestCase(_zeroGuid)]
	[TestCase(_randomGuid)]
	public void PlaceOffer_WrongWriter_ShouldThrow(Guid id)
    {
        AssertThrows<KeyNotFoundException>(new PlaceOfferCommand(
			AddStock().Id, id, 1, OfferType.Buy, 1M));
        _mediator.VerifyNoOtherCalls();
    }

    [Test]
    [TestCase(0)]
    [TestCase(-2)]
    [TestCase(-99)]
    public void PlaceOffer_WrongAmount_ShouldThrow(int amount)
    {
        var command = new PlaceOfferCommand(AddStock().Id,
            AddUser().Id, amount, OfferType.Buy, 1M);
        AssertThrows<ArgumentOutOfRangeException>(command);
        _mediator.VerifyNoOtherCalls();
    }

    [Test]
    public void PlaceOffer_WrongType_ShouldThrow()
    {
        var user = AddUser();
        user.Funds = 10M;
        _ctx.SaveChanges();
        var command = new PlaceOfferCommand(AddStock().Id,
            user.Id, 1, OfferType.PublicOfferring, 1M);
        AssertThrows<PublicOfferingException>(command);
        _mediator.VerifyNoOtherCalls();
    }

    [Test]
    [TestCase(0)]
    [TestCase(-2)]
    [TestCase(-9.9)]
    public void PlaceOffer_WrongPrice_ShouldThrow(decimal price)
    {
        var command = new PlaceOfferCommand(AddStock().Id,
            AddUser().Id, 1, OfferType.Buy, price);
        AssertThrows<ArgumentOutOfRangeException>(command);
        _mediator.VerifyNoOtherCalls();
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void PlaceSellOffer_NotEnoughStocks_ShouldThrow(bool owningStocks)
    {
        //Arrange
        var ownedAmount = 5;
        var offerAmount = 10;
        Assert.Greater(offerAmount, ownedAmount);

        var user = AddUser();
        var stock = AddStock();

        if (owningStocks)
        {
            _ctx.Add(new Share
            {
                Amount = ownedAmount,
                Owner = user,
                Stock = stock
            });
            _ctx.SaveChanges();
        }

        var command = new PlaceOfferCommand(stock.Id,
            user.Id, offerAmount, OfferType.Sell, 1M);

        //Act & Assert
        AssertThrows<NoStocksOnSellerException>(command);
        _mediator.VerifyNoOtherCalls();
    }

    [Test]
    public void PlaceOffer_NoFunds_ShouldThrow()
    {
        var command = new PlaceOfferCommand(AddStock().Id,
            AddUser().Id, 1, OfferType.Buy, 1M);
        AssertThrows<InsufficientFundsException>(command);
        _mediator.VerifyNoOtherCalls();
    }

    [Test]
    public void PlaceSellOffer_NoOtherOffers_ShouldAddOffer()
    {
        //Arrange
        var price = 1M;
        var ownedAmount = 10;
        var offerAmount = 5;
        Assert.Greater(ownedAmount, offerAmount);

        var user = AddUser();
        var stock = AddStock();

        _ctx.Add(new Share
        {
            Amount = ownedAmount,
            Owner = user,
            Stock = stock
        });
        _ctx.SaveChanges();

        //Act
        Handle(new PlaceOfferCommand(stock.Id, user.Id,
            offerAmount, OfferType.Sell, price));

        //Assert
        Assert.AreEqual(1, _ctx.TradeOffer.Count());

        var offer = _ctx.TradeOffer.First();
        Assert.AreEqual(price, offer.Price);
        Assert.AreEqual(OfferType.Sell, offer.Type);
        Assert.AreEqual(offerAmount, offer.Amount);
        _mediator.VerifyNoOtherCalls();
    }

    [Test]
    public void PlaceSellOffer_ExistingOffers_ShouldNotAddOffer()
    {
        //Arrange
        var sellAmount = 5;
        var buyAmount = 4;
        Assert.Positive(sellAmount);
        Assert.Positive(buyAmount);
        Assert.Greater(buyAmount * 2, sellAmount);

        var price1 = 1M;
        var price2 = 2M;
        var price3 = 3M;
        Assert.Greater(price2, price1);
        Assert.Greater(price3, price2);

        var user = AddUser();
        var stock = AddStock();

        _ctx.Add(new Share
        {
            Amount = sellAmount,
            Owner = user,
            Stock = stock
        });

        var existingOffer1 = new TradeOffer
        {
            Amount = buyAmount,
            Price = price1,
            StockId = stock.Id,
            Type = OfferType.Buy,
            WriterId = AddUser().Id
        };
        _ctx.Add(existingOffer1);

        var existingOffer2 = new TradeOffer
        {
            Amount = buyAmount,
            Price = price2,
            StockId = stock.Id,
            Type = OfferType.Buy,
            WriterId = AddUser().Id
        };
        _ctx.Add(existingOffer2);

        var existingOffer3 = new TradeOffer
        {
            Amount = buyAmount,
            Price = price2,
            StockId = stock.Id,
            Type = OfferType.Buy,
            WriterId = AddUser().Id
        };
        _ctx.Add(existingOffer3);

        _ctx.SaveChanges();

        //Act
        Handle(new PlaceOfferCommand(stock.Id, user.Id,
            sellAmount, OfferType.Sell, price2));

        //Assert
        var offers = _ctx.TradeOffer.ToList();
        Assert.True(offers.All(x => x.Type == OfferType.Buy));
        Assert.AreEqual(3, offers.Count);
        VerifyMock();
    }

    [Test]
    public void PlaceBuyOffer_NoOtherOffers_ShouldAddOffer()
    {
        //Arrange
        var user = AddUser();
        var price = 1M;
        var amount = 2;
        user.Funds = price * amount;
        Assert.Positive(amount);
        Assert.Positive(price);
        _ctx.SaveChanges();

        //Act
        Handle(new PlaceOfferCommand(AddStock().Id, user.Id,
            amount, OfferType.Buy, price));

        //Assert
        _mediator.VerifyNoOtherCalls();

        Assert.AreEqual(1, _ctx.TradeOffer.Count());

        Assert.AreEqual(1, _ctx.TradeOffer.Count());

        var offer = _ctx.TradeOffer.First();
        Assert.AreEqual(price, offer.Price);
        Assert.AreEqual(OfferType.Buy, offer.Type);
        Assert.AreEqual(amount, offer.Amount);
    }

    [Test]
    public void PlaceBuyOffer_ExistingOffers_ShouldNotAddOffer()
    {
        //Arrange
        var user = AddUser();
        var sellAmount = 5;
        var buyAmount = 7;
        Assert.Positive(sellAmount);
        Assert.Positive(buyAmount);
        Assert.Greater(sellAmount * 2, buyAmount);

        var price1 = 1M;
        var price2 = 2M;
        user.Funds = buyAmount * price2;
        Assert.Greater(price2, price1);

        var stock = AddStock();

        var existingOffer1 = new TradeOffer
        {
            Amount = sellAmount,
            Price = price1,
            StockId = stock.Id,
            Type = OfferType.Sell,
            WriterId = AddUser().Id
        };
        _ctx.Add(existingOffer1);

        var existingOffer2 = new TradeOffer
        {
            Amount = sellAmount,
            Price = price2,
            StockId = stock.Id,
            Type = OfferType.PublicOfferring
        };
        _ctx.Add(existingOffer2);

        _ctx.SaveChanges();

        //Act
        Handle(new PlaceOfferCommand(stock.Id, user.Id,
            buyAmount, OfferType.Buy, price2));

        //Assert
        var offers = _ctx.TradeOffer.ToList();
        Assert.True(offers.All(x => x.Type != OfferType.Buy));
        Assert.AreEqual(2, offers.Count);
        VerifyMock();
    }

    private void VerifyMock()
    {
        _mediator.Verify(x => x.Send(It.Is<AcceptOfferCommand>(
            c => c.Amount == null), CancellationToken.None), Times.Once());
        _mediator.Verify(x => x.Send(It.Is<AcceptOfferCommand>(
            c => c.Amount != null), CancellationToken.None), Times.Once());
        _mediator.VerifyNoOtherCalls();
    }
}
