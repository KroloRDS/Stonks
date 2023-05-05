using System;
using System.Linq;
using MediatR;
using NUnit.Framework;
using Stonks.Data.Models;
using Stonks.CQRS.Queries.Common;

namespace UnitTests.CQRS.Queries.Common;

public class GetStockPricesTest : 
	QueryTest<GetStockPricesQuery, GetStockPricesResponse>
{
	protected override IRequestHandler<GetStockPricesQuery,
		GetStockPricesResponse> GetHandler()
	{
		return new GetStockPricesQueryHandler(_readOnlyCtx);
	}

	[Test]
	public void GetHistoricalPrices_OverlapingDates_ShouldThrow()
	{
		AssertThrows<ArgumentOutOfRangeException>(new GetStockPricesQuery
		{
			StockId = AddStock().Id,
			FromDate = new DateTime(2021, 1, 1),
			ToDate = new DateTime(2020, 1, 1)
		});
	}

	[Test]
	[TestCase(null, null, 3)]
	[TestCase("2020.6.6", null, 2)]
	[TestCase(null, "2020.6.6", 1)]
	[TestCase("2022.1.1", null, 1)]
	[TestCase("2019.6.6", "2020.6.6", 1)]
	[TestCase("2019.6.6", "2021.1.1", 2)]
	[TestCase("2021.6.6", "2021.7.7", 0)]
	[TestCase("2001.6.6", "2019.7.7", 0)]
	[TestCase("2025.1.1", null, 0)]
	[TestCase(null, "2015.1.1", 0)]
	public void GetHistoricalPrices_SingleStock_PositiveTest(DateTime? from,
		DateTime? to, int expectedCount)
	{
		//Arrange
		var stockId = AddStock().Id;
		var historicalPrices = new[]
		{
			new AvgPrice
			{
				StockId = stockId,
				DateTime = new DateTime(2020, 1, 1)
			},
			new AvgPrice
			{
				StockId = stockId,
				DateTime = new DateTime(2021, 1, 1)
			},
			new AvgPrice
			{
				StockId = stockId,
				DateTime = new DateTime(2022, 1, 1)
			},
		};
		_ctx.AddRange(historicalPrices);
		_ctx.SaveChanges();

		//Act & Assert
		Assert.AreEqual(expectedCount, GetPricesCount(stockId, from, to));
	}

	[Test]
	public void GetHistoricalPrices_MultipleStocks_PositiveTest()
	{
		//Arrange
		var stock1 = AddStock().Id;
		var stock2 = AddStock().Id;
		var historicalPrices = new[]
		{
			new AvgPrice
			{
				StockId = stock1,
				DateTime = new DateTime(2020, 1, 1)
			},
			new AvgPrice
			{
				StockId = stock2,
				DateTime = new DateTime(2021, 1, 1)
			},
		};
		_ctx.AddRange(historicalPrices);
		_ctx.SaveChanges();

		//Act & Assert
		Assert.AreEqual(1, GetPricesCount(stock1, null, null));
		Assert.AreEqual(2, GetPricesCount(null, null, null));
	}

	private int GetPricesCount(Guid? stockId, DateTime? from, DateTime? to)
	{
		return Handle(new GetStockPricesQuery
		{
			StockId = stockId,
			FromDate = from,
			ToDate = to
		}).Prices.Count();
	}
}
