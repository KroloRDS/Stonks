using System;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using Stonks.Models;
using Stonks.Managers.Bankruptcy;

namespace UnitTests.Managers.BattleRoyale;

[TestFixture]
public class StockOwnershipManagerTests : ManagerTest
{
	private readonly BankruptSharesManager _manager;

	public StockOwnershipManagerTests()
	{
		_manager = new BankruptSharesManager(_ctx);
	}

	[Test]
	public void GetAllOwnedStocksAmount_NullStock_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(() => _manager.GetAllSharesAmount(null));
	}

	[Test]
	public void GetAllOwnedStocksAmount_WrongStock_ShouldThrow()
	{
		Assert.Zero(_manager.GetAllSharesAmount(Guid.NewGuid()));
	}

	[Test]
	public void GetAllOwnedStocksAmount_PositiveTest()
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
		var actual = _manager.GetAllSharesAmount(stock.Id);

		//Assert
		Assert.AreEqual(amount1 + amount2, actual);
	}

	[Test]
	public void RemoveAllOwnershipForStock_NullStock_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(
			() => _manager.RemoveAllShares(null));
	}

	[Test]
	public void RemoveAllOwnershipForStock_WrongStock_ShouldThrow()
	{
		Assert.Throws<KeyNotFoundException>(
			() => _manager.RemoveAllShares(Guid.NewGuid()));
	}

	[Test]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(2)]
	[TestCase(14)]
	public void RemoveAllOwnershipForStock_PositiveTest(int ownerships)
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

		//Act & Assert
		_manager.RemoveAllShares(stockId);
		Assert.False(_ctx.Share.Any());
	}
}
