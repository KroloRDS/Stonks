using System;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using Stonks.DTOs;
using Stonks.Managers;

namespace UnitTests.Managers;

[TestFixture]
public class StockOwnershipManagerTests : ManagerTest
{
	private readonly StockOwnershipManager _manager;

	public StockOwnershipManagerTests()
	{
		_manager = new StockOwnershipManager(_ctx);
	}

	[Test]
	public void BuyStock_NullParameter_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(() => _manager.BuyStock(null));
	}

	[Test]
	public void BuyStock_NullAmount_ShouldThrow()
	{
		var command = new BuyStockCommand
		{
			BuyerId = GetUserId(AddUser()),
			StockId = AddStock().Id,
			Amount = null
		};

		Assert.Throws<ArgumentNullException>(() => _manager.BuyStock(command));
	}

	[Test]
	[TestCase(0)]
	[TestCase(-2)]
	[TestCase(-99)]
	public void BuyStock_WrongAmount_ShouldThrow(int amount)
	{
		var command = new BuyStockCommand
		{
			BuyerId = GetUserId(AddUser()),
			StockId = AddStock().Id,
			Amount = amount
		};

		Assert.Throws<ArgumentOutOfRangeException>(() => _manager.BuyStock(command));
	}

	[Test]
	public void BuyStock_NullBuyer_ShouldThrow()
	{
		var command = new BuyStockCommand
		{
			BuyerId = null,
			StockId = AddStock().Id,
			Amount = 5
		};

		Assert.Throws<ArgumentNullException>(() => _manager.BuyStock(command));
	}

	[Test]
	public void BuyStock_WrongBuyer_ShouldThrow()
	{
		var command = new BuyStockCommand
		{
			BuyerId = Guid.NewGuid(),
			StockId = AddStock().Id,
			Amount = 5
		};

		Assert.Throws<KeyNotFoundException>(() => _manager.BuyStock(command));
	}

	[Test]
	public void BuyStock_NullStock_ShouldThrow()
	{
		var command = new BuyStockCommand
		{
			BuyerId = GetUserId(AddUser()),
			StockId = null,
			Amount = 5
		};

		Assert.Throws<ArgumentNullException>(() => _manager.BuyStock(command));
	}

	[Test]
	public void BuyStock_WrongStock_ShouldThrow()
	{
		var command = new BuyStockCommand
		{
			BuyerId = GetUserId(AddUser()),
			StockId = Guid.NewGuid(),
			Amount = 5
		};

		Assert.Throws<KeyNotFoundException>(() => _manager.BuyStock(command));
	}

	[Test]
	public void BuyStock_BuyNotFromUser_SellerNotNull_ShouldThrow()
	{
		var command = new BuyStockCommand
		{
			BuyerId = GetUserId(AddUser()),
			SellerId = GetUserId(AddUser()),
			StockId = AddStock().Id,
			Amount = 5
		};

		Assert.Throws<ArgumentException>(() => _manager.BuyStock(command));
	}

	[Test]
	public void BuyStock_NotEnoughPublicStocks_ShouldThrow()
	{
		var command = new BuyStockCommand
		{
			BuyerId = GetUserId(AddUser()),
			StockId = AddStock(0).Id,
			Amount = 5
		};

		Assert.Throws<InvalidOperationException>(() => _manager.BuyStock(command));
	}

	[Test]
	public void BuyStock_NoStocksOnSeller_ShouldThrow()
	{
		//Arrange
		var sellerId = GetUserId(AddUser());
		var stockId = AddStock().Id;

		var command = new BuyStockCommand
		{
			BuyerId = GetUserId(AddUser()),
			SellerId = GetUserId(AddUser()),
			BuyFromUser = true,
			StockId = stockId,
			Amount = 5
		};

		//Act & Assert
		Assert.Throws<InvalidOperationException>(() => _manager.BuyStock(command));
	}

	[Test]
	public void BuyStock_NotEnoughStocksOnSeller_ShouldThrow()
	{
		//Arrange
		var sellerInitialStocks = 5;
		var buyerStocks = 100;
		Assert.Greater(buyerStocks, sellerInitialStocks);

		var sellerId = GetUserId(AddUser());
		var stockId = AddStock().Id;

		_manager.BuyStock(new BuyStockCommand
		{
			BuyerId = sellerId,
			StockId = stockId,
			Amount = sellerInitialStocks
		});

		var command = new BuyStockCommand
		{
			BuyerId = GetUserId(AddUser()),
			SellerId = sellerId,
			BuyFromUser = true,
			StockId = stockId,
			Amount = buyerStocks
		};

		//Act & Assert
		Assert.Throws<InvalidOperationException>(() => _manager.BuyStock(command));
	}

	[Test]
	public void BuyStock_PositiveTest()
	{
		//Part 1 - buy public stocks

		//Arrange
		var publicStocks = 100;
		var sellerInitialStocks = 10;
		var buyerStocks = 5;
		Assert.Greater(publicStocks, sellerInitialStocks);
		Assert.Greater(sellerInitialStocks, buyerStocks);

		var buyerId = GetUserId(AddUser());
		var sellerId = GetUserId(AddUser());
		var stock = AddStock(publicStocks);

		//Act
		_manager.BuyStock(new BuyStockCommand
		{
			BuyerId = sellerId,
			StockId = stock.Id,
			Amount = sellerInitialStocks
		});

		//Assert
		var sellerActualStocks = GetAmountOfOwnedStocks(sellerId, stock.Id);
		Assert.AreEqual(sellerInitialStocks, sellerActualStocks);
		Assert.AreEqual(publicStocks - sellerInitialStocks, stock.PublicallyOfferredAmount);
		Assert.AreEqual(1, GetTransactionCount(sellerId, null, stock.Id));

		//Part 2 - buy stocks from user

		//Arrange & Act
		_manager.BuyStock(new BuyStockCommand
		{
			BuyerId = buyerId,
			SellerId = sellerId,
			BuyFromUser = true,
			StockId = stock.Id,
			Amount = buyerStocks
		});

		//Assert
		sellerActualStocks = GetAmountOfOwnedStocks(sellerId, stock.Id);
		var buyerActualStocks = GetAmountOfOwnedStocks(buyerId, stock.Id);

		Assert.AreEqual(sellerInitialStocks - buyerStocks, sellerActualStocks);
		Assert.AreEqual(buyerStocks, buyerActualStocks);
		Assert.AreEqual(publicStocks - sellerInitialStocks, stock.PublicallyOfferredAmount);
		Assert.AreEqual(1, GetTransactionCount(sellerId, null, stock.Id));
		Assert.AreEqual(1, GetTransactionCount(buyerId, sellerId, stock.Id));
	}

	private int GetAmountOfOwnedStocks(Guid userId, Guid stockId)
	{
		var ownership = _ctx.GetStockOwnership(userId.ToString(), stockId);
		return ownership is null ? 0 : ownership.Amount;
	}

	private int GetTransactionCount(Guid buyerId, Guid? sellerId, Guid stockId)
	{
		if (sellerId == null)
		{
			return _ctx.Transaction.Where(x =>
				x.Buyer.Id == buyerId.ToString() && x.Stock.Id == stockId)
				.Count();
		}

		return _ctx.Transaction.Where(x =>
			x.Buyer.Id == buyerId.ToString() && x.Stock.Id == stockId &&
			x.Seller != null && x.Seller.Id == sellerId.ToString())
			.Count();
	}
}
