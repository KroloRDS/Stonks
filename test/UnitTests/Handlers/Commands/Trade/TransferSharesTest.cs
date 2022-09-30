using System;
using System.Linq;
using System.Collections.Generic;
using MediatR;
using NUnit.Framework;
using Stonks.Helpers;
using Stonks.Requests.Commands.Trade;

namespace UnitTests.Handlers.Commands.Trade;

public class TransferSharesTest : CommandTest<TransferSharesCommand>
{
	protected override IRequestHandler<TransferSharesCommand, Unit>
		GetHandler()
	{
		return new TransferSharesCommandHandler(_ctx);
	}

	[Test]
	[TestCase(0)]
	[TestCase(-2)]
	[TestCase(-99)]
	public void TransferShares_WrongAmount_ShouldThrow(int amount)
	{
		var command = new TransferSharesCommand(
			AddStock().Id, amount, GetUserId(AddUser()), false);
		AssertThrows<ArgumentOutOfRangeException>(command);
	}

	[Test]
	public void TransferShares_WrongBuyer_ShouldThrow()
	{
		var stockId = AddStock().Id;
		var command1 = new TransferSharesCommand(
			stockId, 1, default, false);
		var command2 = new TransferSharesCommand(
			stockId, 1, Guid.NewGuid(), false);
		AssertThrows<KeyNotFoundException>(command1);
		AssertThrows<KeyNotFoundException>(command2);
	}

	[Test]
	public void TransferShares_NullSeller_ShouldThrow()
	{
		var command = new TransferSharesCommand(
			AddStock().Id, 1, GetUserId(AddUser()), true, null);
		AssertThrows<ArgumentNullException>(command);
	}

	[Test]
	public void TransferShares_WrongSeller_ShouldThrow()
	{
		var command = new TransferSharesCommand(
			AddStock().Id, 1, GetUserId(AddUser()), true, Guid.NewGuid());
		AssertThrows<KeyNotFoundException>(command);
	}

	[Test]
	public void TransferShares_WrongStock_ShouldThrow()
	{
		var userId = GetUserId(AddUser());
		var command1 = new TransferSharesCommand(
			default, 1, userId, false);
		var command2 = new TransferSharesCommand(
			Guid.NewGuid(), 1, userId, false);
		AssertThrows<KeyNotFoundException>(command1);
		AssertThrows<KeyNotFoundException>(command2);
	}

	[Test]
	public void TransferShares_BuyNotFromUser_SellerNotNull_ShouldThrow()
	{
		var command = new TransferSharesCommand(AddStock().Id,
			1, GetUserId(AddUser()), false, GetUserId(AddUser()));
		AssertThrows<ExtraRefToSellerException>(command);
	}

	[Test]
	public void TransferShares_NotEnoughPublicStocks_ShouldThrow()
	{
		var command = new TransferSharesCommand(
			AddStock(0).Id, 5, GetUserId(AddUser()), false);
		AssertThrows<NoPublicStocksException>(command);
	}

	[Test]
	public void TransferShares_BankruptStock_ShouldThrow()
	{
		var command = new TransferSharesCommand(
			AddBankruptStock().Id, 5, GetUserId(AddUser()), false);
		AssertThrows<BankruptStockException>(command);
	}

	[Test]
	public void TransferShares_NoStocksOnSeller_ShouldThrow()
	{
		var command = new TransferSharesCommand(AddStock().Id,
			1, GetUserId(AddUser()), true, GetUserId(AddUser()));
		AssertThrows<NoStocksOnSellerException>(command);
	}

	[Test]
	public void TransferShares_NotEnoughStocksOnSeller_ShouldThrow()
	{
		//Arrange
		var sellerInitialStocks = 5;
		var buyerStocks = 100;
		Assert.Greater(buyerStocks, sellerInitialStocks);

		var sellerId = GetUserId(AddUser());
		var stockId = AddStock().Id;
		Handle(new TransferSharesCommand(
			stockId, sellerInitialStocks, sellerId, false));

		var command = new TransferSharesCommand(stockId,
			buyerStocks, GetUserId(AddUser()), true, sellerId);

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

		var buyerId = GetUserId(AddUser());
		var sellerId = GetUserId(AddUser());
		var stock = AddStock(publicStocks);

		//Act
		Handle(new TransferSharesCommand(stock.Id,
			sellerInitialStocks, sellerId, false));

		//Assert
		var sellerActualStocks = GetAmountOfOwnedStocks(sellerId, stock.Id);
		Assert.AreEqual(sellerInitialStocks, sellerActualStocks);
		Assert.AreEqual(publicStocks - sellerInitialStocks,
			stock.PublicallyOfferredAmount);
		Assert.AreEqual(1, GetTransactionCount(sellerId, null, stock.Id));

		//Part 2 - buy stocks from user

		//Arrange & Act
		Handle(new TransferSharesCommand(stock.Id,
			buyerStocks, buyerId, true, sellerId));

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
		var ownership = _ctx.GetShares(userId, stockId);
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
