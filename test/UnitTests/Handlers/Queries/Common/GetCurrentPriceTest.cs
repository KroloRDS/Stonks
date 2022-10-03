using System;
using System.Collections.Generic;
using MediatR;
using NUnit.Framework;

using Stonks.Models;
using Stonks.Responses.Common;
using Stonks.Requests.Queries.Common;
using Stonks.CustomExceptions;

namespace UnitTests.Handlers.Queries.Common;

public class GetCurrentPriceTest :
	QueryTest<GetCurrentPriceQuery, GetCurrentPriceResponse>
{
	protected override IRequestHandler<GetCurrentPriceQuery,
		GetCurrentPriceResponse> GetHandler()
	{
		return new GetCurrentPriceQueryHandler(_ctx);
	}

	[Test]
	[TestCase(default)]
	[TestCase("0f2e2e31-972b-4440-a91f-e290089292a2")]
	public void GetCurrentPrice_WrongStock_ShouldThrow(Guid id)
	{
		AssertThrows<KeyNotFoundException>(new GetCurrentPriceQuery(id));
	}

	[Test]
	public void GetCurrentPrice_BankruptStock_ShouldThrow()
	{
		AssertThrows<BankruptStockException>(
			new GetCurrentPriceQuery(AddBankruptStock().Id));
	}

	[Test]
	public void GetCurrentPrice_MultiplePrices_ShouldThrow()
	{
		//Arrange
		var stockId = AddStock().Id;
		_ctx.AddRange(new[]
		{
			new AvgPriceCurrent
			{
				StockId = stockId,
				Id = Guid.NewGuid()
			},
			new AvgPriceCurrent
			{
				StockId = stockId,
				Id = Guid.NewGuid()
			} 
		});

		//Act & Assert
		AssertThrows<InvalidOperationException>(
			new GetCurrentPriceQuery(stockId));
	}

	[Test]
	public void GetCurrentPrice_PositiveTest()
	{
		//Arrange
		var stockId = AddStock().Id;
		var price = 5m;
		_ctx.Add(new AvgPriceCurrent
		{
			StockId = stockId,
			Price = price
		});
		_ctx.SaveChanges();

		//Act & Assert
		Assert.AreEqual(price,Handle(new GetCurrentPriceQuery(stockId)).Price);
	}
}