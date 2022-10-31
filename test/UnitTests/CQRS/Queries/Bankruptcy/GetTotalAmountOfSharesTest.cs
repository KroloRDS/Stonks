using System;
using MediatR;
using NUnit.Framework;
using Stonks.Data.Models;
using Stonks.CQRS.Queries.Bankruptcy;

namespace UnitTests.CQRS.Queries.Bankruptcy;

public class GetTotalAmountOfSharesTest :
	QueryTest<GetTotalAmountOfSharesQuery, GetTotalAmountOfSharesResponse>
{
	protected override IRequestHandler<GetTotalAmountOfSharesQuery,
		GetTotalAmountOfSharesResponse> GetHandler()
	{
		return new GetTotalAmountOfSharesQueryHandler(_ctx);
	}

	[Test]
	[TestCase(default)]
	[TestCase("164f5fb6-41ec-4182-91ec-4a0bfd34c0de")]
	public void GetAllSharesAmount_WrongStock_ShouldBeZero(Guid id)
	{
		Assert.Zero(Handle(new GetTotalAmountOfSharesQuery(id)).Amount);
	}

	[Test]
	public void GetAllSharesAmount_PositiveTest()
	{
		//Arrange
		var amount1 = 5;
		var amount2 = 10;
		Assert.Positive(amount1);
		Assert.Positive(amount2);

		var stockId = AddStock().Id;
		var user1 = AddUser();
		var user2 = AddUser();

		_ctx.AddRange(new[]
		{
			new Share
			{
				Amount = amount1,
				Owner = user1,
				StockId = stockId
			},
			new Share
			{
				Amount = amount2,
				Owner = user2,
				StockId = stockId
			}
		});
		_ctx.SaveChanges();

		//Act
		var actual = Handle(new GetTotalAmountOfSharesQuery(stockId)).Amount;

		//Assert
		Assert.AreEqual(amount1 + amount2, actual);
	}
}
