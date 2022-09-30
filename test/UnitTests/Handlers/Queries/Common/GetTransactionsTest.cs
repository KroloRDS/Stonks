using System;
using System.Linq;
using System.Collections.Generic;
using MediatR;
using NUnit.Framework;

using Stonks.Models;
using Stonks.Responses.Common;
using Stonks.Requests.Queries.Common;

namespace UnitTests.Handlers.Queries.Common;

public class GetTransactionsTest
	: QueryTest<GetTransactionsQuery, GetTransactionsResponse>
{
	protected override IRequestHandler<GetTransactionsQuery,
		GetTransactionsResponse> GetHandler()
	{
		return new GetTransactionsQueryHandler(_ctx);
	}

	[Test]
	[TestCase(default)]
	[TestCase("a9c8020e-87c6-4acb-b8fa-34487791024c")]
	public void GetTransactions_WrongStock_ShouldThrow(Guid id)
	{
		AssertThrows<KeyNotFoundException>(new GetTransactionsQuery(id));
	}

	[Test]
	public void GetTransactions_NoTransactions_ShouldReturnEmptyList()
	{
		Assert.False(Handle(new GetTransactionsQuery(AddStock().Id))
			.Transactions.Any());
	}

	[Test]
	public void GetTransactions_PositiveTest()
	{
		//Arrange
		var date1 = new DateTime(2001, 1, 1);
		var date2 = new DateTime(2002, 1, 1);
		Assert.Greater(date2, date1);

		//Arrange
		var stockId = AddStock().Id;
		var buyerId = AddUser().Id;
		_ctx.AddRange(new[]
		{
			new Transaction
			{
				StockId = stockId,
				BuyerId = buyerId,
				Timestamp = date1
			},
			new Transaction
			{
				StockId = stockId,
				BuyerId = buyerId,
				Timestamp = date2
			}
		});
		_ctx.SaveChanges();

		//Assert
		Assert.AreEqual(2, Handle(new GetTransactionsQuery(stockId))
			.Transactions.Count());
		Assert.AreEqual(1, Handle(new GetTransactionsQuery(stockId, date2))
			.Transactions.Count());
		Assert.False(Handle(new GetTransactionsQuery(stockId, date2.AddDays(1)))
			.Transactions.Any());
	}
}
