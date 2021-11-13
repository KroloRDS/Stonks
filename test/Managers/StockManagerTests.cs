using NUnit.Framework;
using Stonks.Managers;
using Stonks.Helpers;
using Stonks.DTOs;
using System;
using Stonks.Models;
using Microsoft.AspNetCore.Identity;

namespace UnitTests.Managers;

[TestFixture]
public class StockManagerTests : UsingContext
{
	private StockManager manager;

	[SetUp]
	public void SetUp()
	{
		manager = new StockManager(ctx);
	}

	[Test]
	public void BuyStock_NullParameter_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(() => manager.BuyStock(null));
	}

	[Test]
	public void BuyStock_NullAmount_ShouldThrow()
	{
		var dto = new BuyStockDTO
		{
			BuyerId = GetUserId(AddUser()),
			StockId = AddStock().Id,
			Amount = null
		};

		Assert.Throws<ArgumentNullException>(() => manager.BuyStock(dto));
	}

	[Test]
	[TestCase(0)]
	[TestCase(-2)]
	[TestCase(-99)]
	public void BuyStock_WrongAmount_ShouldThrow(int amount)
	{
		var dto = new BuyStockDTO
		{
			BuyerId = GetUserId(AddUser()),
			StockId = AddStock().Id,
			Amount = amount
		};

		Assert.Throws<ArgumentOutOfRangeException>(() => manager.BuyStock(dto));
	}

	private Stock AddStock(int publicAmount = 100)
	{
		var stock = new Stock
		{
			Symbol = "TEST",
			Name = "TestStock",
			Price = 1M,
			PublicallyOfferredAmount = publicAmount
		};
		ctx.Add(stock);
		ctx.SaveChanges();
		return stock;
	}

	private IdentityUser AddUser()
	{
		var user = new IdentityUser();
		ctx.Add(user);
		ctx.SaveChanges();
		return user;
	}

	private Guid GetUserId(IdentityUser user)
	{
		return Guid.Parse(user.Id);
	}
}
