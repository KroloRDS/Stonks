using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Moq;
using MediatR;
using NUnit.Framework;
using Stonks.Models;
using Stonks.Responses.Common;
using Stonks.Requests.Queries.Common;
using Stonks.Requests.Commands.Common;

namespace UnitTests.Handlers.Commands.Common;

public class UpdateAveragePriceTest :
	CommandTest<UpdateAveragePriceCommand>
{
	protected override IRequestHandler<UpdateAveragePriceCommand, Unit>
		GetHandler()
	{
		return new UpdateAveragePriceCommandHandler(_ctx, _mediator.Object);
	}

	[Test]
	public void UpdateAveragePrice_WrongStock_ShouldThrow()
	{
		AssertThrows<KeyNotFoundException>(
			new UpdateAveragePriceCommand(default));
		AssertThrows<KeyNotFoundException>(
			new UpdateAveragePriceCommand(Guid.NewGuid()));
	}

	[Test]
	public void UpdateAveragePrice_BankruptStock_ShouldDoNothing()
	{
		Handle(new UpdateAveragePriceCommand(AddBankruptStock().Id));
		Assert.False(_ctx.AvgPriceCurrent.Any());
		_mediator.VerifyNoOtherCalls();
	}

	[Test]
	public void UpdateAveragePrice_NoData()
	{
		//Arrange
		var stockId = AddStock().Id;
		SetupTransactions(Array.Empty<(int, decimal)>());

		//Act
		Handle(new UpdateAveragePriceCommand(stockId));
		var priceRecord = GetCurrentPrice(stockId);

		//Assert
		Assert.AreEqual(0UL, priceRecord.SharesTraded);
		Assert.AreEqual(UpdateAveragePriceCommandHandler.DEFAULT_PRICE,
			priceRecord.Price);
		VerifyMediator(1);
	}

	[Test]
	public void UpdateAveragePrice_NoNewTransactions()
	{
		//Arrange
		var sharesTraded = 100;
		var averagePrice = 5M;
		Assert.Positive(sharesTraded);
		Assert.Positive(averagePrice);

		var stockId = AddStock().Id;
		AddCurrentPrice(stockId, (ulong)sharesTraded, averagePrice);
		SetupTransactions(Array.Empty<(int, decimal)>());

		//Act
		Handle(new UpdateAveragePriceCommand(stockId));
		var oldPriceRecord = _ctx.AvgPrice.FirstOrDefault();
		var newPriceRecord = GetCurrentPrice(stockId);

		//Assert
		Assert.NotNull(oldPriceRecord);
		Assert.NotNull(newPriceRecord);

		Assert.AreNotEqual(newPriceRecord.Created, oldPriceRecord!.DateTime);

		Assert.AreEqual(averagePrice, oldPriceRecord.Price);
		Assert.AreEqual(averagePrice, newPriceRecord.Price);

		Assert.AreEqual(sharesTraded, oldPriceRecord.SharesTraded);
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
		var priceRecord = GetCurrentPrice(stockId);

		//Assert
		Assert.AreEqual(expectedAmount, priceRecord.SharesTraded);
		Assert.AreEqual(expectedAverage, priceRecord.Price);
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
		var oldPriceRecord = _ctx.AvgPrice.FirstOrDefault();
		var newPriceRecord = GetCurrentPrice(stockId);

		//Assert
		Assert.NotNull(oldPriceRecord);
		Assert.NotNull(newPriceRecord);

		Assert.AreNotEqual(newPriceRecord.Created, oldPriceRecord!.DateTime);

		Assert.AreEqual(averagePrice, oldPriceRecord.Price);
		Assert.AreEqual(expectedAverage, newPriceRecord.Price);

		Assert.AreEqual(amountTraded, oldPriceRecord.SharesTraded);
		Assert.AreEqual(expectedAmount, newPriceRecord.SharesTraded);
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

	private void AddCurrentPrice(Guid stockId,
		ulong sharesTraded, decimal averagePrice)
	{
		_ctx.Add(new AvgPriceCurrent
		{
			StockId = stockId,
			Created = DateTime.Now,
			SharesTraded = sharesTraded,
			Price = averagePrice,
		});
		_ctx.SaveChanges();
	}

	private AvgPriceCurrent GetCurrentPrice(Guid stockId)
	{
		return _ctx.AvgPriceCurrent.Single(x => x.StockId == stockId);
	}
}
