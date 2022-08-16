using System;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using Stonks.Models;
using Stonks.Managers.Bankruptcy;

namespace UnitTests.Managers.BattleRoyale;

[TestFixture]
public class BankruptSharesManagerTests : ManagerTest
{
	private readonly BankruptSharesManager _manager;

	public BankruptSharesManagerTests()
	{
		_manager = new BankruptSharesManager(_ctx);
	}

	[Test]
	public void GetAllSharesAmount_NullStock_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(() => _manager.GetTotalAmountOfShares(null));
	}

	[Test]
	public void GetAllSharesAmount_WrongStock_ShouldThrow()
	{
		Assert.Zero(_manager.GetTotalAmountOfShares(Guid.NewGuid()));
	}

	[Test]
	public void GetAllSharesAmount_PositiveTest()
	{
		//Arrange
		var amount1 = 5;
		var amount2 = 10;
		Assert.Positive(amount1);
		Assert.Positive(amount2);

		var stock = AddStock();
		var user1 = AddUser();
		var user2 = AddUser();

		_ctx.Add(new Share
		{
			Amount = amount1,
			Owner = user1,
			Stock = stock
		});

		_ctx.Add(new Share
		{
			Amount = amount2,
			Owner = user2,
			Stock = stock
		});
		_ctx.SaveChanges();

		//Act
		var actual = _manager.GetTotalAmountOfShares(stock.Id);

		//Assert
		Assert.AreEqual(amount1 + amount2, actual);
	}

	[Test]
	public void RemoveAllShares_NullStock_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(
			() => _manager.RemoveAllShares(null));
	}

	[Test]
	public void RemoveAllShares_WrongStock_ShouldThrow()
	{
		Assert.Throws<KeyNotFoundException>(
			() => _manager.RemoveAllShares(Guid.NewGuid()));
	}

	[Test]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(2)]
	[TestCase(14)]
	public void RemoveAllShares_PositiveTest(int ownerships)
	{
		//Arange
		var stockId = AddStock().Id;
		for (int i = 0; i < ownerships; i++)
		{
			_ctx.Add(new Share
			{
				StockId = stockId,
				OwnerId = AddUser().Id
			});
		}
		_ctx.SaveChanges();
		Assert.AreEqual(ownerships > 0, _ctx.Share.Any());

		//Act
		_manager.RemoveAllShares(stockId);
		_ctx.SaveChanges();

		//Assert
		Assert.False(_ctx.Share.Any());
	}
}
