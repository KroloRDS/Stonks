using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using Moq;
using MediatR;
using NUnit.Framework;

using Stonks.Data.Models;
using Stonks.CQRS.Commands;
using Stonks.CQRS.Queries.Common;

namespace UnitTests.CQRS.Commands;

public class UpdateAveragePriceTest :
    CommandTest<UpdateAveragePriceCommand>
{
    protected override IRequestHandler<UpdateAveragePriceCommand, Unit>
        GetHandler()
    {
        return new UpdateAveragePriceCommandHandler(_ctx, _mediator.Object);
    }

    [Test]
	[TestCase(default)]
	[TestCase(_zeroGuid)]
	[TestCase(_randomGuid)]
	public void UpdateAveragePrice_WrongStock_ShouldThrow(Guid id)
    {
        AssertThrows<KeyNotFoundException>(new UpdateAveragePriceCommand(id));
    }

    [Test]
    public void UpdateAveragePrice_BankruptStock_ShouldDoNothing()
    {
		var id = AddBankruptStock().Id;
		Handle(new UpdateAveragePriceCommand(id));
        Assert.Null(_ctx.GetById<Stock>(id).Result.LastPriceUpdate);
        _mediator.VerifyNoOtherCalls();
    }

    [Test]
    public void UpdateAveragePrice_NoData()
    {
        //Arrange
		var now = DateTime.Now;
        var stockId = AddStock().Id;
        SetupTransactions(Array.Empty<(int, decimal)>());

        //Act
        Handle(new UpdateAveragePriceCommand(stockId));
        var stock = _ctx.GetById<Stock>(stockId).Result;
		var price = _ctx.AvgPrice.Single(x => x.StockId == stockId);

        //Assert
        Assert.AreEqual(0UL, stock.SharesTraded);
        Assert.AreEqual(Stock.DEFAULT_PRICE, stock.Price);
		Assert.Greater(stock.LastPriceUpdate, now);

		Assert.AreEqual(0UL, price.SharesTraded);
		Assert.AreEqual(Stock.DEFAULT_PRICE, price.Price);
		Assert.Greater(price.DateTime, now);
		VerifyMediator(1);
    }

    [Test]
    public void UpdateAveragePrice_NoNewTransactions()
    {
        //Arrange
        ulong sharesTraded = 100;
        var averagePrice = 5M;
        Assert.Positive(sharesTraded);
        Assert.Positive(averagePrice);

		var now = DateTime.Now;

        var stock = AddStock();
		stock.SharesTraded = sharesTraded;
		stock.LastPriceUpdate = now;
		stock.Price = averagePrice;

		_ctx.Add(new AvgPrice
		{
			StockId = stock.Id,
			SharesTraded = sharesTraded,
			Price = averagePrice,
			DateTime = now
		});

        SetupTransactions(Array.Empty<(int, decimal)>());
		_ctx.SaveChanges();

		//Act
		Handle(new UpdateAveragePriceCommand(stock.Id));
        var newPriceRecord = _ctx.AvgPrice
			.OrderByDescending(x => x.DateTime).First();

		//Assert
		stock = _ctx.GetById<Stock>(stock.Id).Result;

		Assert.Greater(newPriceRecord.DateTime, now);
        Assert.Greater(stock.LastPriceUpdate, now);

        Assert.AreEqual(averagePrice, newPriceRecord.Price);
        Assert.AreEqual(averagePrice, stock.Price);

        Assert.AreEqual(sharesTraded, stock.SharesTraded);
        Assert.AreEqual(sharesTraded, newPriceRecord.SharesTraded);

        VerifyMediator(1);
    }

    [Test]
    public void UpdateAveragePrice_OnlyNewTransactions()
    {
        //Arrange
        var stockId = AddStock().Id;

        var prices = new (int Amount, decimal Price)[]
        {
            new (10, 10M),
            new (20, 20M),
            new (30, 30M),
            new (11, 10M)
        };

        foreach ((var amount, var price) in prices)
        {
            Assert.Positive(amount);
            Assert.Positive(price);
        }
        SetupTransactions(prices);

        var expectedAmount = prices.Sum(x => x.Amount);
        var expectedAverage = prices.Sum(x => x.Amount * x.Price) / expectedAmount;

        //Act
        Handle(new UpdateAveragePriceCommand(stockId));
		var priceRecord = _ctx.AvgPrice.First();
		var stock = _ctx.GetById<Stock>(stockId).Result;

		//Assert
		Assert.AreEqual(expectedAmount, priceRecord.SharesTraded);
		Assert.AreEqual(expectedAmount, stock.SharesTraded);

		Assert.AreEqual(expectedAverage, priceRecord.Price);
		Assert.AreEqual(expectedAverage, stock.Price);
		VerifyMediator(1);
    }

    [Test]
    public void UpdateAveragePrice_OldAndNewTransactions()
    {
        //Arrange
        var stockId = AddStock().Id;
        var amountTraded = 100;
        var averagePrice = 5M;
        Assert.Positive(amountTraded);
        Assert.Positive(averagePrice);

        SetupTransactions(new[] { (amountTraded, averagePrice) });
        Handle(new UpdateAveragePriceCommand(stockId));

        var prices = new (int Amount, decimal Price)[]
        {
            new (amountTraded, averagePrice),
            new (10, 10M),
            new (20, 20M),
            new (30, 30M),
            new (11, 10M)
        };

        foreach ((var amount, var price) in prices)
        {
            Assert.Positive(amount);
            Assert.Positive(price);
        }
        SetupTransactions(prices);

        var expectedAmount = prices.Sum(x => x.Amount) + amountTraded;
        var expectedAverage = (prices.Sum(x => x.Amount * x.Price) +
            amountTraded * averagePrice) / expectedAmount;

        //Act
        Handle(new UpdateAveragePriceCommand(stockId));
		var pricesRecords = _ctx.AvgPrice.OrderBy(x => x.DateTime).ToArray();
		var oldPrice = pricesRecords[0];
		var newPrice = pricesRecords[1];
		var stock = _ctx.GetById<Stock>(stockId).Result;

		//Assert
		Assert.Greater(newPrice.DateTime, oldPrice.DateTime);
        Assert.AreEqual(averagePrice, oldPrice.Price);
        Assert.AreEqual(expectedAverage, newPrice.Price);

        Assert.AreEqual(amountTraded, oldPrice.SharesTraded);
        Assert.AreEqual(expectedAmount, newPrice.SharesTraded);
        VerifyMediator(2);
    }

    private void SetupTransactions((int Amount, decimal Price)[] prices)
    {
        _mediator.Setup(x => x.Send(
            It.IsAny<GetTransactionsQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetTransactionsResponse(
                prices.Select(x => new Transaction
                {
                    Amount = x.Amount,
                    Price = x.Price
                })));
    }

    private void VerifyMediator(int times)
    {
        _mediator.Verify(x => x.Send(
            It.IsAny<GetTransactionsQuery>(), CancellationToken.None),
            Times.Exactly(times));
        _mediator.VerifyNoOtherCalls();
    }
}
