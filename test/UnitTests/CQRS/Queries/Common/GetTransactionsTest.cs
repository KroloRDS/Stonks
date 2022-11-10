using System;
using System.Linq;
using System.Collections.Generic;
using MediatR;
using NUnit.Framework;
using Stonks.Data.Models;
using Stonks.CQRS.Queries.Common;

namespace UnitTests.CQRS.Queries.Common;

public class GetTransactionsTest
	: QueryTest<GetTransactionsQuery, GetTransactionsResponse>
{
	protected override IRequestHandler<GetTransactionsQuery,
		GetTransactionsResponse> GetHandler()
	{
		return new GetTransactionsQueryHandler(_readOnlyCtx);
	}

	[Test]
	public void GetTransactions_NoTransactions_ShouldReturnEmptyList()
	{
		Assert.False(Handle(new GetTransactionsQuery()).Transactions.Any());
	}

	[Test]
	[TestCase(false, false, false, 5)]
	[TestCase(true, false, false, 4)]
	[TestCase(false, true, false, 4)]
	[TestCase(false, false, true, 4)]
	[TestCase(true, true, false, 3)]
	[TestCase(true, false, true, 3)]
	[TestCase(false, true, true, 3)]
	[TestCase(true, true, true, 2)]
	public void GetTransactions_PositiveTest(bool useStockId,
		bool useDate, bool useUserId, int expectedCount)
	{
		//Arrange
		var date = new DateTime(2001, 1, 1);
		var stockId = AddStock().Id;
		var userId = AddUser().Id;
		_ctx.AddRange(new[]
		{
			new Transaction
			{
				StockId = stockId,
				BuyerId = userId,
				Timestamp = date
			},
			new Transaction
			{
				StockId = AddStock().Id,
				BuyerId = userId,
				Timestamp = date
			},
			new Transaction
			{
				StockId = stockId,
				BuyerId = AddUser().Id,
				SellerId = userId,
				Timestamp = date
			},
			new Transaction
			{
				StockId = stockId,
				BuyerId = AddUser().Id,
				Timestamp = date
			},
			new Transaction
			{
				StockId = stockId,
				BuyerId = userId,
				Timestamp = date.AddDays(-1)
			}
		});
		_ctx.SaveChanges();

		//Act & Assert
		var query = new GetTransactionsQuery
		{
			StockId = useStockId ? stockId : null,
			FromDate = useDate ? date : null,
			UserId = useUserId ? userId : null
		};
		Assert.AreEqual(expectedCount, Handle(query).Transactions.Count());
	}
}
