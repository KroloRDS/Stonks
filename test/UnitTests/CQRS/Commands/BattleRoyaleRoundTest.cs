using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using Moq;
using MediatR;
using NUnit.Framework;

using Stonks.Data.Models;
using Stonks.CQRS.Helpers;
using Stonks.CQRS.Commands;
using Stonks.CQRS.Queries.Bankruptcy.GetWeakestStock;

namespace UnitTests.CQRS.Commands;

public class BattleRoyaleRoundTest : CommandTest<BattleRoyaleRoundCommand>
{
    private readonly Mock<IAddPublicOffers> _addOffers = new();
	private readonly BattleRoyaleRoundCommandHandler _battleRoyaleHandler;

	public BattleRoyaleRoundTest()
	{
		_battleRoyaleHandler = new BattleRoyaleRoundCommandHandler(
			_ctx, _mediator.Object, _config.Object, _addOffers.Object);
	}

	protected override IRequestHandler<BattleRoyaleRoundCommand, Unit> GetHandler()
	{
		return new BattleRoyaleRoundCommandHandler(_ctx,
			_mediator.Object, _config.Object, _addOffers.Object);
	}

    [Test]
    public void BattleRoyaleRound_PositiveTest()
    {
        //Arrange
        _config.Setup(x => x.NewStocksAfterRound()).Returns(1);
        _mediator.Setup(x => x.Send(
            It.IsAny<GetWeakestStockIdQuery>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetWeakestStockIdResponse(AddStock().Id));

		//Act
		Handle(new BattleRoyaleRoundCommand());

        //Assert
        VerifyMocks();
    }

    private void VerifyMocks()
    {
        _mediator.Verify(x => x.Send(It.IsAny<GetWeakestStockIdQuery>(),
            It.IsAny<CancellationToken>()), Times.Once());
        _mediator.VerifyNoOtherCalls();
        _addOffers.Verify(x => x.Handle(It.IsAny<int>(), It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Once());
        _addOffers.VerifyNoOtherCalls();
        _config.Verify(x => x.NewStocksAfterRound(), Times.Once());
        _config.VerifyNoOtherCalls();
    }

    [Test]
    [TestCase(default)]
    [TestCase(_zeroGuid)]
    [TestCase(_randomGuid)]
    public void Bankrupt_WrongStock_ShouldThrow(Guid id)
    {
		Assert.ThrowsAsync<KeyNotFoundException>(() =>
			_battleRoyaleHandler.Bankrupt(id));
	}

    [Test]
    public void Bankrupt_PositiveTest()
    {
        var stock = AddStock();
		_battleRoyaleHandler.Bankrupt(stock.Id).Wait();
        Assert.IsTrue(stock.Bankrupt);
        Assert.Zero(stock.PublicallyOfferredAmount);
        Assert.NotNull(stock.BankruptDate);
    }

	[Test]
	[TestCase(default)]
	[TestCase(_zeroGuid)]
	[TestCase(_randomGuid)]
	public void RemoveSharesAndOffers_WrongStock_ShouldThrow(Guid id)
    {
        Assert.ThrowsAsync<KeyNotFoundException>(() =>
			_battleRoyaleHandler.RemoveSharesAndOffers(
				id, CancellationToken.None));
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(14)]
    public void RemoveAllOffersForStock_PositiveTest(int count)
    {
        //Arange
        var stockId = AddStock().Id;
        var range = Enumerable.Range(1, count);
        var offers = range.Select(x => new TradeOffer { StockId = stockId });
        var shares = range.Select(x => new Share
        {
            StockId = stockId,
            OwnerId = Guid.NewGuid()
        });

        _ctx.AddRange(offers);
        _ctx.AddRange(shares);
        _ctx.SaveChanges();

        Assert.AreEqual(count > 0, _ctx.Share.Any());
        Assert.AreEqual(count > 0, _ctx.TradeOffer.Any());

		//Act
		_battleRoyaleHandler.RemoveSharesAndOffers(
			stockId, CancellationToken.None).Wait();
        _ctx.SaveChanges();

        //Assert
        Assert.False(_ctx.Share.Any());
        Assert.False(_ctx.TradeOffer.Any());
    }

    [Test]
    [TestCase(0)]
    [TestCase(-2)]
    [TestCase(-99)]
    public void UpdatePublicallyOfferedAmount_WrongAmount_ShouldThrow(int amount)
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
			_battleRoyaleHandler.UpdatePublicallyOfferedAmount(
                amount, AddStock().Id, CancellationToken.None));
    }

    [Test]
    public void UpdatePublicallyOfferedAmount_PositiveTest()
    {
        //Arrange
        var amount = 100;
        var smallerAmount = 50;
        var biggerAmount = 150;

        //Check if test data makes sense
        Assert.Positive(amount);
        Assert.Positive(smallerAmount);
        Assert.Positive(biggerAmount);
        Assert.Greater(biggerAmount, amount);
        Assert.Greater(amount, smallerAmount);

        var stock1 = AddStock(0);
        var stock2 = AddStock(smallerAmount);
        var stock3 = AddStock(biggerAmount);
        var excludedStock = AddStock(0);
        var bankruptStock = AddBankruptStock();

		//Act
		_battleRoyaleHandler.UpdatePublicallyOfferedAmount(
			amount, excludedStock.Id, CancellationToken.None).Wait();
        _ctx.SaveChanges();

        //Assert
        Assert.AreEqual(amount, GetPublicAmount(stock1.Id));
        Assert.AreEqual(amount, GetPublicAmount(stock2.Id));
        Assert.AreEqual(biggerAmount, GetPublicAmount(stock3.Id));
        Assert.Zero(GetPublicAmount(excludedStock.Id));
        Assert.Zero(GetPublicAmount(bankruptStock.Id));
    }

    private int GetPublicAmount(Guid stockId)
    {
        return _ctx.GetById<Stock>(stockId).Result.PublicallyOfferredAmount;
    }
}
