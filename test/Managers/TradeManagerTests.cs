using System;
using System.Linq;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Identity;

using Stonks.DTOs;
using Stonks.Models;
using Stonks.Managers;

namespace UnitTests.Managers;

[TestFixture]
public class TradeManagerTests : ManagerTest
{
	private readonly TradeManager _manager;

	public TradeManagerTests()
	{
		var mockStockManager = new Mock<IStockManager>();
		_manager = new TradeManager(mockStockManager.Object, _ctx);
	}

	[Test]
	public void RemoveOffer_NullOffer_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(() => _manager.RemoveOffer(null));
	}

	[Test]
	public void RemoveOffer_WrongOffer_ShouldThrow()
	{
		Assert.Throws<KeyNotFoundException>(() => _manager.RemoveOffer(Guid.NewGuid()));
	}

	[Test]
	public void RemoveOffer_PositiveTest()
	{
		var offer = AddOffer(OfferType.Buy);
		_manager.RemoveOffer(offer.Id);
		Assert.Zero(_ctx.TradeOffer.Count());
	}

	[Test]
	public void AcceptOffer_NullOffer_ShouldThrow()
	{
		var userId = GetUserId(AddUser());
		Assert.Throws<ArgumentNullException>(() => _manager.AcceptOffer(userId, null));
		Assert.Throws<ArgumentNullException>(() => _manager.AcceptOffer(userId, null, 1));
	}

	[Test]
	public void AcceptOffer_WrongOffer_ShouldThrow()
	{
		var userId = GetUserId(AddUser());
		Assert.Throws<KeyNotFoundException>(() => _manager.AcceptOffer(userId, Guid.NewGuid()));
		Assert.Throws<KeyNotFoundException>(() => _manager.AcceptOffer(userId, Guid.NewGuid(), 1));
	}

	[Test]
	public void AcceptOffer_NullUser_ShouldThrow()
	{
		var offer = AddOffer();
		Assert.Throws<ArgumentNullException>(() => _manager.AcceptOffer(null, offer.Id));
		Assert.Throws<ArgumentNullException>(() => _manager.AcceptOffer(null, offer.Id, 1));
	}

	[Test]
	public void AcceptOffer_WrongUser_ShouldThrow()
	{
		var offer = AddOffer();
		Assert.Throws<KeyNotFoundException>(() => _manager.AcceptOffer(Guid.NewGuid(), offer.Id));
		Assert.Throws<KeyNotFoundException>(() => _manager.AcceptOffer(Guid.NewGuid(), offer.Id, 1));
	}

	[Test]
	[TestCase(0)]
	[TestCase(-2)]
	[TestCase(-99)]
	public void AcceptOffer_WrongAmount_ShouldThrow(int amount)
	{
		var offer = AddOffer();
		Assert.Throws<ArgumentOutOfRangeException>(() => _manager.AcceptOffer(GetUserId(AddUser()), offer.Id, amount));
	}

	[Test]
	[TestCase(OfferType.Buy)]
	[TestCase(OfferType.Sell)]
	[TestCase(OfferType.PublicOfferring)]
	public void AcceptOffer_FullAmount_ShouldRemoveOffer(OfferType type)
	{
		//Arrange
		var offer1 = AddOffer(type);
		var offer2 = AddOffer(type);
		var amount = 99;
		Assert.Greater(amount, offer2.Amount);

		//Act
		_manager.AcceptOffer(GetUserId(AddUser()), offer1.Id);
		_manager.AcceptOffer(GetUserId(AddUser()), offer2.Id, amount);

		//Assert
		Assert.Zero(_ctx.TradeOffer.Count());
	}

	[Test]
	[TestCase(OfferType.Buy)]
	[TestCase(OfferType.Sell)]
	[TestCase(OfferType.PublicOfferring)]
	public void AcceptOffer_NotFullAmount_ShouldNotRemoveOffer(OfferType type)
	{
		//Arrange
		var offer = AddOffer(type);
		var amount = 5;
		var initialAmout = offer.Amount;
		Assert.Greater(initialAmout, amount);

		//Act
		_manager.AcceptOffer(GetUserId(AddUser()), offer.Id, amount);

		//Assert
		Assert.AreEqual(1, _ctx.TradeOffer.Count());
		Assert.AreEqual(initialAmout - amount, offer.Amount);
	}

	[Test]
	public void PlaceOffer_NullParameter_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(() => _manager.PlaceOffer(null));
	}

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

		Assert.Throws<ArgumentOutOfRangeException>(() => _manager.PlaceOffer(command));
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

		Assert.Throws<ArgumentException>(() => _manager.PlaceOffer(command));
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

		Assert.Throws<ArgumentOutOfRangeException>(() => _manager.PlaceOffer(command));
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
			_ctx.Add(new StockOwnership
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

		Assert.Throws<ArgumentException>(() => _manager.PlaceOffer(command));
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

		_ctx.Add(new StockOwnership
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

		_ctx.Add(new StockOwnership
		{
			Amount = sellAmount,
			Owner = user,
			Stock = stock
		});

		_ctx.Add(new TradeOffer
		{
			Amount = buyAmount,
			BuyPrice = price1,
			StockId = stock.Id,
			Type = OfferType.Buy,
			WriterId = AddUser().Id
		});

		_ctx.Add(new TradeOffer
		{
			Amount = buyAmount,
			BuyPrice = price2,
			StockId = stock.Id,
			Type = OfferType.Buy,
			WriterId = AddUser().Id
		});

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
		Assert.AreEqual(1, _ctx.TradeOffer.Count());

		var offer = _ctx.TradeOffer.First();
		Assert.AreEqual(OfferType.Buy, offer.Type);
		Assert.AreEqual(price1, offer.BuyPrice);
		Assert.AreEqual(buyAmount * 2 - sellAmount, offer.Amount);
	}

	[Test]
	public void PlaceBuyOffer_NoOtherOffers_ShouldAddOffer()
	{
		//Arrange
		var price = 1M;
		var amount = 2;
		Assert.Positive(amount);
		Assert.Positive(price);

		var command = new PlaceOfferCommand
		{
			WriterId = GetUserId(AddUser()),
			StockId = AddStock().Id,
			Price = price,
			Type = OfferType.Buy,
			Amount = amount
		};

		//Act
		_manager.PlaceOffer(command);

		//Assert
		Assert.AreEqual(1, _ctx.TradeOffer.Count());

		var offer = _ctx.TradeOffer.First();
		Assert.AreEqual(price, offer.BuyPrice);
		Assert.AreEqual(OfferType.Buy, offer.Type);
		Assert.AreEqual(amount, offer.Amount);
	}

	[Test]
	public void PlaceBuyOffer_ExistingOffers_ShouldNotAddOffer()
	{
		//Arrange
		var sellAmount = 5;
		var buyAmount = 7;
		Assert.Positive(sellAmount);
		Assert.Positive(buyAmount);
		Assert.Greater(sellAmount * 2, buyAmount);

		var price1 = 1M;
		var price2 = 2M;
		Assert.Greater(price2, price1);

		var stock = AddStock();

		_ctx.Add(new TradeOffer
		{
			Amount = sellAmount,
			SellPrice = price1,
			StockId = stock.Id,
			Type = OfferType.Sell,
			WriterId = AddUser().Id
		});

		_ctx.Add(new TradeOffer
		{
			Amount = sellAmount,
			SellPrice = price2,
			StockId = stock.Id,
			Type = OfferType.Sell,
			WriterId = AddUser().Id
		});

		_ctx.SaveChanges();

		var command = new PlaceOfferCommand
		{
			WriterId = GetUserId(AddUser()),
			StockId = stock.Id,
			Price = price2,
			Type = OfferType.Buy,
			Amount = buyAmount
		};

		//Act
		_manager.PlaceOffer(command);

		//Assert
		Assert.AreEqual(1, _ctx.TradeOffer.Count());

		var offer = _ctx.TradeOffer.First();
		Assert.AreEqual(OfferType.Sell, offer.Type);
		Assert.AreEqual(price2, offer.SellPrice);
		Assert.AreEqual(sellAmount * 2 - buyAmount, offer.Amount);
	}

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
}