using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using NUnit.Framework;

using Stonks.Util;
using Stonks.CQRS.Helpers;

namespace UnitTests.CQRS.Helpers;

public class TransferSharesTest : InMemoryDb
{
	private readonly TransferShares _transferShares;

	public TransferSharesTest()
	{
		_transferShares = new TransferShares(_ctx);
	}

    [Test]
    [TestCase(0)]
    [TestCase(-2)]
    [TestCase(-99)]
    public void TransferShares_WrongAmount_ShouldThrow(int amount)
    {
		var command = new TransferSharesCommand
		{
			StockId = AddStock().Id,
			Amount = amount,
			BuyerId = AddUser().Id,
			BuyFromUser = false
		};
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
			() => _transferShares.Handle(command, CancellationToken.None));
    }

    [Test]
    public void TransferShares_WrongBuyer_ShouldThrow()
    {
        var stockId = AddStock().Id;
		var command1 = new TransferSharesCommand
		{
			StockId = stockId,
			Amount = 1,
			BuyerId = default,
			BuyFromUser = false
		};
		var command2 = new TransferSharesCommand
		{
			StockId = stockId,
			Amount = 1,
			BuyerId = Guid.NewGuid(),
			BuyFromUser = false
		};
        AssertThrows<KeyNotFoundException>(command1);
        AssertThrows<KeyNotFoundException>(command2);
    }

    [Test]
    public void TransferShares_NullSeller_ShouldThrow()
    {
		var command = new TransferSharesCommand
		{
			StockId = AddStock().Id,
			Amount = 1,
			BuyerId = AddUser().Id,
			BuyFromUser = true,
			SellerId = null
		};
		AssertThrows<ArgumentNullException>(command);
    }

    [Test]
    public void TransferShares_WrongSeller_ShouldThrow()
    {
		var command = new TransferSharesCommand
		{
			StockId = AddStock().Id,
			Amount = 1,
			BuyerId = AddUser().Id,
			BuyFromUser = true,
			SellerId = Guid.NewGuid()
		};
		AssertThrows<KeyNotFoundException>(command);
    }

    [Test]
    public void TransferShares_WrongStock_ShouldThrow()
    {
        var userId = AddUser().Id;
		var command1 = new TransferSharesCommand
		{
			StockId = Guid.NewGuid(),
			Amount = 1,
			BuyerId = userId,
			BuyFromUser = false
		};
		var command2 = new TransferSharesCommand
		{
			StockId = default,
			Amount = 1,
			BuyerId = userId,
			BuyFromUser = false
		};
		AssertThrows<KeyNotFoundException>(command1);
        AssertThrows<KeyNotFoundException>(command2);
    }

    [Test]
    public void TransferShares_BuyNotFromUser_SellerNotNull_ShouldThrow()
    {
		var command = new TransferSharesCommand
		{
			StockId = AddStock().Id,
			Amount = 1,
			BuyerId = AddUser().Id,
			BuyFromUser = false,
			SellerId = AddUser().Id
		};
		AssertThrows<ExtraRefToSellerException>(command);
    }

    [Test]
    public void TransferShares_NotEnoughPublicStocks_ShouldThrow()
    {
		var command = new TransferSharesCommand
		{
			StockId = AddStock(0).Id,
			Amount = 5,
			BuyerId = AddUser().Id,
			BuyFromUser = false
		};
		AssertThrows<NoPublicStocksException>(command);
    }

    [Test]
    public void TransferShares_BankruptStock_ShouldThrow()
    {
		var command = new TransferSharesCommand
		{
			StockId = AddBankruptStock().Id,
			Amount = 5,
			BuyerId = AddUser().Id,
			BuyFromUser = false
		};
		AssertThrows<BankruptStockException>(command);
    }

    [Test]
    public void TransferShares_NoStocksOnSeller_ShouldThrow()
    {
		var command = new TransferSharesCommand
		{
			StockId = AddStock().Id,
			Amount = 1,
			BuyerId = AddUser().Id,
			BuyFromUser = true,
			SellerId = AddUser().Id
		};
		AssertThrows<NoStocksOnSellerException>(command);
    }

    [Test]
    public void TransferShares_NotEnoughStocksOnSeller_ShouldThrow()
    {
        //Arrange
        var sellerInitialStocks = 5;
        var buyerStocks = 100;
        Assert.Greater(buyerStocks, sellerInitialStocks);

        var sellerId = AddUser().Id;
        var stockId = AddStock().Id;
		Handle(new TransferSharesCommand
		{
			StockId = stockId,
			Amount = sellerInitialStocks,
			BuyerId = sellerId,
			BuyFromUser = false
		});

        var command = new TransferSharesCommand
		{
			StockId = stockId,
			Amount = buyerStocks,
			BuyerId = AddUser().Id,
			BuyFromUser = true,
			SellerId = sellerId
		};

        //Act & Assert
        AssertThrows<NoStocksOnSellerException>(command);
    }

    [Test]
    public void TransferShares_PositiveTest()
    {
        //Part 1 - buy public stocks

        //Arrange
        var publicStocks = 100;
        var sellerInitialStocks = 10;
        var buyerStocks = 5;
        Assert.Greater(publicStocks, sellerInitialStocks);
        Assert.Greater(sellerInitialStocks, buyerStocks);

        var buyerId = AddUser().Id;
        var sellerId = AddUser().Id;
        var stock = AddStock(publicStocks);

        //Act
		Handle(new TransferSharesCommand
		{
			StockId = stock.Id,
			Amount = sellerInitialStocks,
			BuyerId = sellerId,
			BuyFromUser = false
		});

		//Assert
		var sellerActualStocks = GetAmountOfOwnedStocks(sellerId, stock.Id);
        Assert.AreEqual(sellerInitialStocks, sellerActualStocks);
        Assert.AreEqual(publicStocks - sellerInitialStocks,
            stock.PublicallyOfferredAmount);
        Assert.AreEqual(1, GetTransactionCount(sellerId, null, stock.Id));

        //Part 2 - buy stocks from user

        //Arrange & Act
		Handle(new TransferSharesCommand
		{
			StockId = stock.Id,
			Amount = buyerStocks,
			BuyerId = buyerId,
			BuyFromUser = true,
			SellerId = sellerId
		});

		//Assert
		sellerActualStocks = GetAmountOfOwnedStocks(sellerId, stock.Id);
        var buyerActualStocks = GetAmountOfOwnedStocks(buyerId, stock.Id);

        Assert.AreEqual(sellerInitialStocks - buyerStocks, sellerActualStocks);
        Assert.AreEqual(buyerStocks, buyerActualStocks);
        Assert.AreEqual(publicStocks - sellerInitialStocks,
            stock.PublicallyOfferredAmount);
        Assert.AreEqual(1, GetTransactionCount(sellerId, null, stock.Id));
        Assert.AreEqual(1, GetTransactionCount(buyerId, sellerId, stock.Id));
    }

    private int GetAmountOfOwnedStocks(Guid userId, Guid stockId)
    {
        var ownership = _ctx.GetShares(userId, stockId,
            CancellationToken.None).Result;
        return ownership is null ? 0 : ownership.Amount;
    }

    private int GetTransactionCount(Guid buyerId, Guid? sellerId, Guid stockId)
    {
        if (sellerId == null)
        {
            return _ctx.Transaction.Where(x =>
                x.Buyer.Id == buyerId && x.Stock.Id == stockId)
                .Count();
        }

        return _ctx.Transaction.Where(x =>
            x.Buyer.Id == buyerId && x.Stock.Id == stockId &&
            x.Seller != null && x.Seller.Id == sellerId)
            .Count();
    }

	private void AssertThrows<T>(TransferSharesCommand command)
		where T : Exception
	{
		Assert.ThrowsAsync<T>(
			() => _transferShares.Handle(command, CancellationToken.None));
	}

	private void Handle(TransferSharesCommand command)
	{
		_transferShares.Handle(command, CancellationToken.None).Wait();
		_ctx.SaveChanges();
	}
}
