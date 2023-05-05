using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using Moq;
using MediatR;
using NUnit.Framework;

using Stonks.Data.Models;
using Stonks.CQRS.Queries.Common;

namespace UnitTests.CQRS.Queries.Common;

public class GetStocksViewModelTest :
    QueryTest<GetStocksViewModelQuery, GetStocksViewModelResponse>
{
    protected override IRequestHandler
        <GetStocksViewModelQuery, GetStocksViewModelResponse> GetHandler()
    {
        return new GetStockViewModelQueryHandler(
            _readOnlyCtx, _mediator.Object);
    }

    [Test]
    [TestCase(default)]
    [TestCase(_zeroGuid)]
    [TestCase(_randomGuid)]
    public void GetStocksViewModel_WrongUser_ShouldThrow(Guid id)
    {
        AssertThrows<KeyNotFoundException>(new GetStocksViewModelQuery(id));
    }

    [Test]
    public void GetStocksViewModel_NoDate_ShouldNotThrow()
    {
		SetupMediator(Enumerable.Empty<AvgPrice>(),
			Enumerable.Empty<Transaction>());
		Assert.DoesNotThrow(() => Handle(
            new GetStocksViewModelQuery(AddUser().Id)));
    }

    [Test]
    public void GetStockViewModel_PositiveTest()
    {
        //Arrange
        var userId = AddUser().Id;

        var stock1 = AddStock().Id;
		var stock2 = AddStock().Id;
		SetupMediator(stock1, stock2, userId);

        var stock3 = AddStock().Id;
        _ctx.AddRange(
            new TradeOffer { StockId = stock3, WriterId = userId },
            new TradeOffer { StockId = stock3, WriterId = userId });

        var stock4 = AddStock();
		var name = "stock4";
		stock4.Name = name;
        stock4.Price = 5M;
        var stock4amount = 5;
        _ctx.Add(new Share
        {
            StockId = stock4.Id,
            OwnerId = userId,
            Amount = stock4amount
        });

        _ctx.SaveChanges();

        //Act
        var response = Handle(new GetStocksViewModelQuery(userId)).Stocks;

        //Assert
        Assert.AreEqual(4, response.Count());

        var model = response.First(x => x.Id == stock1);
        Assert.AreEqual(2, model.Transactions.Count());
        Assert.False(model.Prices.Any());
        Assert.False(model.Offers.Any());
        Assert.Zero(model.OwnedAmount);
        Assert.AreEqual(Stock.DEFAULT_PRICE, model.Price);

        model = response.First(x => x.Id == stock2);
        Assert.False(model.Transactions.Any());
        Assert.AreEqual(2, model.Prices.Count());
        Assert.False(model.Offers.Any());
        Assert.Zero(model.OwnedAmount);
        Assert.AreEqual(Stock.DEFAULT_PRICE, model.Price);

        model = response.First(x => x.Id == stock3);
        Assert.False(model.Transactions.Any());
        Assert.False(model.Prices.Any());
        Assert.AreEqual(2, model.Offers.Count());
        Assert.Zero(model.OwnedAmount);
        Assert.AreEqual(Stock.DEFAULT_PRICE, model.Price);

        model = response.First(x => x.Id == stock4.Id);
        Assert.False(model.Transactions.Any());
        Assert.False(model.Prices.Any());
        Assert.False(model.Offers.Any());
		Assert.AreEqual(name, model.Name);
        Assert.AreEqual(stock4amount, model.OwnedAmount);
        Assert.AreEqual(stock4.Price, model.Price);
    }

	private void SetupMediator(Guid stock1, Guid stock2, Guid userId)
	{
		var transactions = new[]
		{
			new Transaction { StockId = stock1, BuyerId = userId },
			new Transaction { StockId = stock1, BuyerId = userId }
		};
		var prices = new[]
		{
			new AvgPrice { StockId = stock2 },
			new AvgPrice { StockId = stock2 }
		};
		SetupMediator(prices, transactions);
	}

	private void SetupMediator(IEnumerable<AvgPrice> prices,
		IEnumerable<Transaction> transactions)
	{
		var token = It.IsAny<CancellationToken>();
		_mediator.Setup(x => x.Send(It.IsAny<GetTransactionsQuery>(), token))
			.ReturnsAsync(new GetTransactionsResponse(transactions));
		_mediator.Setup(x => x.Send(It.IsAny<GetStockPricesQuery>(), token))
			.ReturnsAsync(new GetStockPricesResponse(prices));
	}
}
