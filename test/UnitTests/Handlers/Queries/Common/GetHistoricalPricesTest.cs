using System;
using System.Linq;
using MediatR;
using NUnit.Framework;

using Stonks.Models;
using Stonks.Responses.Common;
using Stonks.Requests.Queries.Common;
using System.Collections.Generic;

namespace UnitTests.Handlers.Queries.Common;

public class GetHistoricalPricesTest : 
	HandlerTest<GetHistoricalPricesQuery, GetHistoricalPricesResponse>
{
	protected override IRequestHandler<GetHistoricalPricesQuery,
		GetHistoricalPricesResponse> GetHandler()
	{
		return new GetHistoricalPricesQueryHandler(_ctx);
	}

	[Test]
	[TestCase(default)]
	[TestCase("8f0f0cef-4451-43ed-8dd6-9c568e862e99")]
	public void GetHistoricalPrices_WrongStock_ShouldThrow(Guid id)
	{
		AssertThrows<KeyNotFoundException>(
			new GetHistoricalPricesQuery{ StockId = id });
	}

	[Test]
	public void GetHistoricalPrices_OverlapingDates_ShouldThrow()
	{
		AssertThrows<ArgumentOutOfRangeException>(new GetHistoricalPricesQuery
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
	public void GetHistoricalPrices_PositiveTest(DateTime? from,
		DateTime? to, int expectedCount)
	{
		//Arrange
		var stock = AddStock();
		var historicalPrices = new[]
		{
			new AvgPrice
			{
				Stock = stock,
				DateTime = new DateTime(2020, 1, 1)
			},
			new AvgPrice
			{
				Stock = stock,
				DateTime = new DateTime(2021, 1, 1)
			},
			new AvgPrice
			{
				Stock = stock,
				DateTime = new DateTime(2022, 1, 1)
			},
		};
		_ctx.AddRange(historicalPrices);
		_ctx.SaveChanges();

		//Act & Assert
		Assert.AreEqual(expectedCount, GetPricesCount(stock.Id, from, to));
	}

	private int GetPricesCount(Guid stockId, DateTime? from, DateTime? to)
	{
		return Handle(new GetHistoricalPricesQuery
		{
			StockId = stockId,
			FromDate = from,
			ToDate = to
		}).Prices.Count();
	}
}
