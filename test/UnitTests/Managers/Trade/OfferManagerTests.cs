using System;
using System.Linq;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using Stonks.DTOs;
using Stonks.Models;
using Stonks.Helpers;
using Stonks.Managers.Trade;
using System.Linq.Expressions;
using Stonks.Requests.Commands.Trade;

namespace UnitTests.Managers.Trade;

[TestFixture]
public class OfferManagerTests : ManagerTest
{
	private readonly OfferManager _manager;
	private readonly Mock<ITransferSharesManager> _mockStockManager = new();
	private readonly Mock<IUserBalanceManager> _mockUserManager = new();

	public OfferManagerTests()
	{
		_manager = new OfferManager(_ctx, _mockUserManager.Object,
			_mockStockManager.Object);
	}

	[Test]
	public void CancelOffer_NullOffer_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(() => _manager.CancelOffer(null));
		VerifyMocksNotCalled();
	}

	[Test]
	public void CancelOffer_WrongOffer_ShouldThrow()
	{
		Assert.Throws<KeyNotFoundException>(
			() => _manager.CancelOffer(Guid.NewGuid()));
		VerifyMocksNotCalled();
	}

	[Test]
	public void CancelOffer_PublicOffering_ShouldThrow()
	{
		var offer = AddOffer(OfferType.PublicOfferring);
		Assert.Throws<PublicOfferingException>(
			() => _manager.CancelOffer(offer.Id));
		VerifyMocksNotCalled();
	}

	[Test]
	public void CancelOffer_SellOffer_ShouldRemove()
	{
		var offer = AddOffer(OfferType.Sell);
		_manager.CancelOffer(offer.Id);
		Assert.False(_ctx.TradeOffer.Any());
		VerifyMocksNotCalled();
	}

	[Test]
	public void CancelOffer_BuyOffer_ShouldRemoveAndRefund()
	{
		var offer = AddOffer(OfferType.Buy);
		_manager.CancelOffer(offer.Id);

		Assert.False(_ctx.TradeOffer.Any());
		VerifyTakeMoneyNotCalled();
		VerifyTransferMoneyNotCalled();
		VerifyTransferSharesMockNotCalled();
		VerifyGiveMoneyCalled(Guid.Parse(offer.WriterId!),
			offer.Price * offer.Amount);
	}

	[Test]
	public void AcceptOffer_NullOffer_ShouldThrow()
	{
		var userId = GetUserId(AddUser());
		Assert.Throws<ArgumentNullException>(
			() => _manager.AcceptOffer(userId, null));
		Assert.Throws<ArgumentNullException>(
			() => _manager.AcceptOffer(userId, null, 1));
		VerifyMocksNotCalled();
	}

	[Test]
	public void AcceptOffer_WrongOffer_ShouldThrow()
	{
		var userId = GetUserId(AddUser());
		Assert.Throws<KeyNotFoundException>(
			() => _manager.AcceptOffer(userId, Guid.NewGuid()));
		Assert.Throws<KeyNotFoundException>(
			() => _manager.AcceptOffer(userId, Guid.NewGuid(), 1));
		VerifyMocksNotCalled();
	}

	[Test]
	public void AcceptOffer_NullUser_ShouldThrow()
	{
		var offer = AddOffer();
		Assert.Throws<ArgumentNullException>(
			() => _manager.AcceptOffer(null, offer.Id));
		Assert.Throws<ArgumentNullException>(
			() => _manager.AcceptOffer(null, offer.Id, 1));
		VerifyMocksNotCalled();
	}

	[Test]
	public void AcceptOffer_WrongUser_ShouldThrow()
	{
		var offer = AddOffer();
		Assert.Throws<KeyNotFoundException>(
			() => _manager.AcceptOffer(Guid.NewGuid(), offer.Id));
		Assert.Throws<KeyNotFoundException>(
			() => _manager.AcceptOffer(Guid.NewGuid(), offer.Id, 1));
		VerifyMocksNotCalled();
	}

	[Test]
	[TestCase(0)]
	[TestCase(-2)]
	[TestCase(-99)]
	public void AcceptOffer_WrongAmount_ShouldThrow(int amount)
	{
		var offer = AddOffer();
		Assert.Throws<ArgumentOutOfRangeException>(
			() => _manager.AcceptOffer(GetUserId(AddUser()), offer.Id, amount));
		VerifyMocksNotCalled();
	}

	[Test]
	[TestCase(OfferType.Buy)]
	[TestCase(OfferType.Sell)]
	[TestCase(OfferType.PublicOfferring)]
	public void AcceptOffer_FullAmount_ShouldRemoveOffer(OfferType type)
	{
		//Arrange
		var clientId1 = GetUserId(AddUser());
		var clientId2 = GetUserId(AddUser());
		var offer1 = AddOffer(type);
		var offer2 = AddOffer(type);
		var amount = 99;
		Assert.Greater(amount, offer2.Amount);

		//Act
		_manager.AcceptOffer(clientId1, offer1.Id);
		_manager.AcceptOffer(clientId2, offer2.Id, amount);

		//Assert
		VerifyAcceptSingleOfferMocksCalled(offer1, clientId1, offer1.Amount);
		VerifyAcceptSingleOfferMocksCalled(offer2, clientId2, offer2.Amount);
		Assert.False(_ctx.TradeOffer.Any());
	}

	[Test]
	[TestCase(OfferType.Buy)]
	[TestCase(OfferType.Sell)]
	[TestCase(OfferType.PublicOfferring)]
	public void AcceptOffer_NotFullAmount_ShouldNotRemoveOffer(OfferType type)
	{
		//Arrange
		var clientId = GetUserId(AddUser());
		var offer = AddOffer(type);
		var amount = 5;
		var initialAmout = offer.Amount;
		Assert.Greater(initialAmout, amount);

		//Act
		_manager.AcceptOffer(clientId, offer.Id, amount);

		//Assert
		VerifyAcceptSingleOfferMocksCalled(offer, clientId, offer.Amount);
		Assert.AreEqual(1, _ctx.TradeOffer.Count());
		Assert.AreEqual(initialAmout - amount, offer.Amount);
	}

	[Test]
	public void PlaceOffer_NullParameter_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(() => _manager.PlaceOffer(null));
		VerifyMocksNotCalled();
	}
/*
	[Test]
	public void PlaceOffer_NullWriter_ShouldThrow()
	{
		var command = new PlaceOfferCommand
		{
			WriterId = null,
			StockId = AddStock().Id,
			Price = 1M,
			Type = OfferType.Buy,
			Amount = 1
		};

		Assert.Throws<ArgumentNullException>(() => _manager.PlaceOffer(command));
		VerifyMocksNotCalled();
	}

	[Test]
	public void PlaceOffer_WrongWriter_ShouldThrow()
	{
		var command = new PlaceOfferCommand
		{
			WriterId = Guid.NewGuid(),
			StockId = AddStock().Id,
			Price = 1M,
			Type = OfferType.Buy,
			Amount = 1
		};

		Assert.Throws<KeyNotFoundException>(() => _manager.PlaceOffer(command));
		VerifyMocksNotCalled();
	}

	[Test]
	public void PlaceOffer_NullStock_ShouldThrow()
	{
		var command = new PlaceOfferCommand
		{
			WriterId = GetUserId(AddUser()),
			StockId = null,
			Price = 1M,
			Type = OfferType.Buy,
			Amount = 1
		};

		Assert.Throws<ArgumentNullException>(() => _manager.PlaceOffer(command));
		VerifyMocksNotCalled();
	}

	[Test]
	public void PlaceOffer_WrongStock_ShouldThrow()
	{
		var command = new PlaceOfferCommand
		{
			WriterId = GetUserId(AddUser()),
			StockId = Guid.NewGuid(),
			Price = 1M,
			Type = OfferType.Buy,
			Amount = 1
		};

		Assert.Throws<KeyNotFoundException>(() => _manager.PlaceOffer(command));
		VerifyMocksNotCalled();
	}

	[Test]
	public void PlaceOffer_BankruptStock_ShouldThrow()
	{
		var command = new PlaceOfferCommand
		{
			WriterId = GetUserId(AddUser()),
			StockId = AddBankruptStock().Id,
			Price = 1M,
			Type = OfferType.Buy,
			Amount = 1
		};

		Assert.Throws<BankruptStockException>(() => _manager.PlaceOffer(command));
		VerifyMocksNotCalled();
	}

	[Test]
	public void PlaceOffer_NullPrice_ShouldThrow()
	{
		var command = new PlaceOfferCommand
		{
			WriterId = GetUserId(AddUser()),
			StockId = AddStock().Id,
			Price = null,
			Type = OfferType.Buy,
			Amount = 1
		};

		Assert.Throws<ArgumentNullException>(() => _manager.PlaceOffer(command));
		VerifyMocksNotCalled();
	}

	[Test]
	[TestCase(0)]
	[TestCase(-2)]
	[TestCase(-9.9)]
	public void PlaceOffer_WrongPrice_ShouldThrow(decimal price)
	{
		var command = new PlaceOfferCommand
		{
			WriterId = GetUserId(AddUser()),
			StockId = AddStock().Id,
			Price = price,
			Type = OfferType.Buy,
			Amount = 1
		};

		Assert.Throws<ArgumentOutOfRangeException>(
			() => _manager.PlaceOffer(command));
		VerifyMocksNotCalled();
	}

	[Test]
	public void PlaceOffer_NullType_ShouldThrow()
	{
		var command = new PlaceOfferCommand
		{
			WriterId = GetUserId(AddUser()),
			StockId = AddStock().Id,
			Price = 1M,
			Type = null,
			Amount = 1
		};

		Assert.Throws<ArgumentNullException>(() => _manager.PlaceOffer(command));
		VerifyMocksNotCalled();
	}

	[Test]
	public void PlaceOffer_WrongType_ShouldThrow()
	{
		var command = new PlaceOfferCommand
		{
			WriterId = GetUserId(AddUser()),
			StockId = AddStock().Id,
			Price = 1M,
			Type = OfferType.PublicOfferring,
			Amount = 1
		};

		Assert.Throws<PublicOfferingException>(
			() => _manager.PlaceOffer(command));
		VerifyMocksNotCalled();
	}

	[Test]
	public void PlaceOffer_NullAmount_ShouldThrow()
	{
		var command = new PlaceOfferCommand
		{
			WriterId = GetUserId(AddUser()),
			StockId = AddStock().Id,
			Price = 1M,
			Type = OfferType.Buy,
			Amount = null
		};

		Assert.Throws<ArgumentNullException>(() => _manager.PlaceOffer(command));
		VerifyMocksNotCalled();
	}

	[Test]
	[TestCase(0)]
	[TestCase(-2)]
	[TestCase(-99)]
	public void PlaceOffer_WrongAmount_ShouldThrow(int amount)
	{
		var command = new PlaceOfferCommand
		{
			WriterId = GetUserId(AddUser()),
			StockId = AddStock().Id,
			Price = 1M,
			Type = OfferType.Buy,
			Amount = amount
		};

		Assert.Throws<ArgumentOutOfRangeException>(
			() => _manager.PlaceOffer(command));
		VerifyMocksNotCalled();
	}

	[Test]
	[TestCase(false)]
	[TestCase(true)]
	public void PlaceSellOffer_NotEnoughStocks_ShouldThrow(bool owningStocks)
	{
		//Arrange
		var ownedAmount = 5;
		var offerAmount = 10;
		Assert.Greater(offerAmount, ownedAmount);

		var user = AddUser();
		var stock = AddStock();

		if (owningStocks)
		{
			_ctx.Add(new Share
			{
				Amount = ownedAmount,
				Owner = user,
				Stock = stock
			});
			_ctx.SaveChanges();
		}

		var command = new PlaceOfferCommand
		{
			WriterId = GetUserId(user),
			StockId = stock.Id,
			Price = 1M,
			Type = OfferType.Sell,
			Amount = offerAmount
		};

		Assert.Throws<NoStocksOnSellerException>(
			() => _manager.PlaceOffer(command));
		VerifyMocksNotCalled();
	}

	[Test]
	public void PlaceSellOffer_NoOtherOffers_ShouldAddOffer()
	{
		//Arrange
		var price = 1M;
		var ownedAmount = 10;
		var offerAmount = 5;
		Assert.Greater(ownedAmount, offerAmount);

		var user = AddUser();
		var stock = AddStock();

		_ctx.Add(new Share
		{
			Amount = ownedAmount,
			Owner = user,
			Stock = stock
		});
		_ctx.SaveChanges();

		var command = new PlaceOfferCommand
		{
			WriterId = GetUserId(user),
			StockId = stock.Id,
			Price = price,
			Type = OfferType.Sell,
			Amount = offerAmount
		};

		//Act
		_manager.PlaceOffer(command);

		//Assert
		Assert.AreEqual(1, _ctx.TradeOffer.Count());

		var offer = _ctx.TradeOffer.First();
		Assert.AreEqual(price, offer.SellPrice);
		Assert.AreEqual(OfferType.Sell, offer.Type);
		Assert.AreEqual(offerAmount, offer.Amount);
		VerifyMocksNotCalled();
	}

	[Test]
	public void PlaceSellOffer_ExistingOffers_ShouldNotAddOffer()
	{
		//Arrange
		var sellAmount = 5;
		var buyAmount = 4;
		Assert.Positive(sellAmount);
		Assert.Positive(buyAmount);
		Assert.Greater(buyAmount * 2, sellAmount);

		var price1 = 1M;
		var price2 = 2M;
		Assert.Greater(price2, price1);

		var user = AddUser();
		var stock = AddStock();

		_ctx.Add(new Share
		{
			Amount = sellAmount,
			Owner = user,
			Stock = stock
		});

		var existingOffer1 = new TradeOffer
		{
			Amount = buyAmount,
			Price = price1,
			StockId = stock.Id,
			Type = OfferType.Buy,
			WriterId = AddUser().Id
		};
		_ctx.Add(existingOffer1);

		var existingOffer2 = new TradeOffer
		{
			Amount = buyAmount,
			Price = price2,
			StockId = stock.Id,
			Type = OfferType.Buy,
			WriterId = AddUser().Id
		};
		_ctx.Add(existingOffer2);

		_ctx.SaveChanges();

		var command = new PlaceOfferCommand
		{
			WriterId = GetUserId(user),
			StockId = stock.Id,
			Price = price1,
			Type = OfferType.Sell,
			Amount = sellAmount
		};

		//Act
		_manager.PlaceOffer(command);

		//Assert
		VerifyTransferSharesMockCalled(existingOffer1,
			GetUserId(user), sellAmount - buyAmount);
		VerifyTransferSharesMockCalled(existingOffer2,
			GetUserId(user), buyAmount);

		VerifyGiveMoneyCalled(GetUserId(user), buyAmount * price2);
		VerifyGiveMoneyCalled(GetUserId(user),
			(sellAmount - buyAmount) * price1);

		VerifyTakeMoneyNotCalled();
		VerifyTransferMoneyNotCalled();

		Assert.AreEqual(1, _ctx.TradeOffer.Count());

		var offer = _ctx.TradeOffer.First();
		Assert.AreEqual(OfferType.Buy, offer.Type);
		Assert.AreEqual(price1, offer.Price);
		Assert.AreEqual(buyAmount * 2 - sellAmount, offer.Amount);
	}

	[Test]
	public void PlaceBuyOffer_NoOtherOffers_ShouldAddOffer()
	{
		//Arrange
		var userId = GetUserId(AddUser());
		var price = 1M;
		var amount = 2;
		Assert.Positive(amount);
		Assert.Positive(price);

		var command = new PlaceOfferCommand
		{
			WriterId = userId,
			StockId = AddStock().Id,
			Price = price,
			Type = OfferType.Buy,
			Amount = amount
		};

		//Act
		_manager.PlaceOffer(command);

		//Assert
		VerifyTakeMoneyCalled(userId, price * amount);
		VerifyGiveMoneyNotCalled();
		VerifyTransferMoneyNotCalled();
		VerifyTransferSharesMockNotCalled();

		Assert.AreEqual(1, _ctx.TradeOffer.Count());

		Assert.AreEqual(1, _ctx.TradeOffer.Count());

		var offer = _ctx.TradeOffer.First();
		Assert.AreEqual(price, offer.Price);
		Assert.AreEqual(OfferType.Buy, offer.Type);
		Assert.AreEqual(amount, offer.Amount);
	}

	[Test]
	public void PlaceBuyOffer_ExistingOffers_ShouldNotAddOffer()
	{
		//Arrange
		var userId = GetUserId(AddUser());
		var sellAmount = 5;
		var buyAmount = 7;
		Assert.Positive(sellAmount);
		Assert.Positive(buyAmount);
		Assert.Greater(sellAmount * 2, buyAmount);

		var price1 = 1M;
		var price2 = 2M;
		Assert.Greater(price2, price1);

		var stock = AddStock();

		var existingOffer1 = new TradeOffer
		{
			Amount = sellAmount,
			SellPrice = price1,
			StockId = stock.Id,
			Type = OfferType.Sell,
			WriterId = AddUser().Id
		};
		_ctx.Add(existingOffer1);

		var existingOffer2 = new TradeOffer
		{
			Amount = sellAmount,
			SellPrice = price2,
			StockId = stock.Id,
			Type = OfferType.PublicOfferring
		};
		_ctx.Add(existingOffer2);

		_ctx.SaveChanges();

		var command = new PlaceOfferCommand
		{
			WriterId = userId,
			StockId = stock.Id,
			Price = price2,
			Type = OfferType.Buy,
			Amount = buyAmount
		};

		//Act
		_manager.PlaceOffer(command);

		//Assert
		VerifyTransferSharesMockCalled(existingOffer1, userId, sellAmount);
		VerifyTransferSharesMockCalled(existingOffer2, userId,
			buyAmount - sellAmount);

		VerifyTransferMoneyCalled(userId, Guid.Parse(existingOffer1.WriterId),
			price1 * sellAmount);
		VerifyTakeMoneyCalled(userId, price2 * (buyAmount - sellAmount));
		VerifyGiveMoneyNotCalled();

		Assert.AreEqual(1, _ctx.TradeOffer.Count());

		var offer = _ctx.TradeOffer.First();
		Assert.AreEqual(OfferType.PublicOfferring, offer.Type);
		Assert.AreEqual(price2, offer.SellPrice);
		Assert.AreEqual(sellAmount * 2 - buyAmount, offer.Amount);
	}
*/
	private TradeOffer AddOffer(OfferType type = OfferType.PublicOfferring)
	{
		var entity = _ctx.Add(new TradeOffer
		{
			Amount = 10,
			SellPrice = 1M,
			StockId = AddStock().Id,
			Type = type,
			WriterId = AddUser().Id
		});
		_ctx.SaveChanges();
		return entity.Entity;
	}

	private void VerifyMocksNotCalled()
	{
		VerifyTransferMoneyNotCalled();
		VerifyTakeMoneyNotCalled();
		VerifyGiveMoneyNotCalled();
		_mockStockManager.Verify(x =>
			x.TransferShares(It.IsAny<TransferSharesCommand>()), Times.Never);
	}

	private void VerifyTransferSharesMockNotCalled()
	{
		_mockStockManager.Verify(x =>
			x.TransferShares(It.IsAny<TransferSharesCommand?>()), Times.Never);
	}

	private void VerifyTransferSharesMockCalled(TradeOffer offer,
		Guid clientId, int amount)
	{
		Guid? writerId = offer.Type == OfferType.PublicOfferring ?
			null : Guid.Parse(offer.WriterId!);
		Guid? sellerId = offer.Type switch
		{
			OfferType.Buy => clientId,
			OfferType.Sell => writerId,
			_ => null,
		};

		Expression<Func<TransferSharesCommand, bool>> match = (x =>
			x.Amount == amount &&
			x.StockId == offer.StockId &&
			x.BuyFromUser == (offer.Type != OfferType.PublicOfferring) &&
			x.BuyerId == (offer.Type == OfferType.Buy ? writerId : clientId) &&
			x.SellerId == sellerId);

		_mockStockManager.Verify(x =>
			x.TransferShares(It.Is(match)), Times.Once);
	}

	private void VerifyAcceptSingleOfferMocksCalled(TradeOffer offer,
		Guid clientId, int amount)
	{
		var offerValue = offer.Type == OfferType.Buy ?
			offer.Price * amount :
			offer.SellPrice * amount;

		VerifyTransferSharesMockCalled(offer, clientId, amount);
		switch (offer.Type)
		{
			case OfferType.Sell:
				var writerId = _ctx.EnsureUserExist(offer.WriterId);
				VerifyTransferMoneyCalled(clientId, writerId, offerValue);
				break;
			case OfferType.Buy:
				VerifyGiveMoneyCalled(clientId, offerValue);
				break;
			case OfferType.PublicOfferring:
				VerifyTakeMoneyCalled(clientId, offerValue);
				break;
		}
	}

	private void VerifyTransferMoneyCalled(Guid? payerId,
		Guid? recipientId, decimal? amount)
	{
		_mockUserManager.Verify(x => 
			x.TransferMoney(payerId, recipientId, amount), Times.Once);
	}

	private void VerifyGiveMoneyCalled(Guid? userId, decimal? amount)
	{
		_mockUserManager.Verify(x =>
			x.GiveMoney(userId, amount), Times.Once);
	}

	private void VerifyTakeMoneyCalled(Guid? userId, decimal? amount)
	{
		_mockUserManager.Verify(x =>
			x.TakeMoney(userId, amount), Times.Once);
	}

	private void VerifyTransferMoneyNotCalled()
	{
		_mockUserManager.Verify(x =>
			x.TransferMoney(
				It.IsAny<Guid?>(),
				It.IsAny<Guid?>(),
				It.IsAny<decimal?>()),
			Times.Never);
	}

	private void VerifyGiveMoneyNotCalled()
	{
		_mockUserManager.Verify(x =>
			x.GiveMoney(It.IsAny<Guid?>(), It.IsAny<decimal?>()), Times.Never);
	}

	private void VerifyTakeMoneyNotCalled()
	{
		_mockUserManager.Verify(x =>
			x.TakeMoney(It.IsAny<Guid?>(), It.IsAny<decimal?>()), Times.Never);
	}

	[TearDown]
	public void ResetMocks()
	{
		_mockUserManager.Reset();
		_mockStockManager.Reset();
	}
}
