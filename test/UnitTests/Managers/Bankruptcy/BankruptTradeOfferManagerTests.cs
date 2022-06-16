﻿using System;
using System.Linq;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using Stonks.Models;
using Stonks.Managers.Common;
using Stonks.Managers.Bankruptcy;

namespace UnitTests.Managers.BattleRoyale;

[TestFixture]
public class BankruptTradeOfferManagerTests : ManagerTest
{
	private readonly BankruptTradeOfferManager _manager;
	private readonly Mock<IGetPriceManager> _mockPriceManager = new();

	public BankruptTradeOfferManagerTests()
	{
		_mockPriceManager.Setup(x => x.GetCurrentPrice(It.IsAny<Guid?>()))
			.Returns(new AvgPriceCurrent
			{
				Amount = IUpdatePriceManager.DEFAULT_PRICE
			});

		_manager = new BankruptTradeOfferManager(_ctx, _mockPriceManager.Object);
	}

	[Test]
	public void AddPublicOffers()
	{
		//Arrange
		var amount = 100;
		var smallerAmount = 50;
		var biggerAmount = 150;

		//Check if test data makes sense
		Assert.Positive(amount);
		Assert.Positive(smallerAmount);
		Assert.Positive(biggerAmount);
		Assert.Greater(biggerAmount, amount);
		Assert.Greater(amount, smallerAmount);

		var stock1 = AddStock();
		var stock2 = AddStock();
		var stock3 = AddStock();
		var bankruptStock = AddBankruptStock();

		_ctx.Add(new TradeOffer
		{
			Amount = smallerAmount,
			StockId = stock2.Id,
			Type = OfferType.PublicOfferring
		});
		_ctx.Add(new TradeOffer
		{
			Amount = biggerAmount,
			StockId = stock3.Id,
			Type = OfferType.PublicOfferring
		});
		_ctx.SaveChanges();

		//Act
		_manager.AddPublicOffers(amount);
		_ctx.SaveChanges();

		//Assert
		_mockPriceManager.Verify(x => x.GetCurrentPrice(stock1.Id), Times.Once);
		_mockPriceManager.Verify(x => x.GetCurrentPrice(stock2.Id), Times.Never);
		_mockPriceManager.Verify(x => x.GetCurrentPrice(stock3.Id), Times.Never);
		_mockPriceManager.Verify(x => 
			x.GetCurrentPrice(bankruptStock.Id), Times.Never);

		Assert.AreEqual(amount, GetPublicOffer(stock1.Id)?.Amount);
		Assert.AreEqual(amount, GetPublicOffer(stock2.Id)?.Amount);
		Assert.AreEqual(biggerAmount, GetPublicOffer(stock3.Id)?.Amount);
		Assert.Null(GetPublicOffer(bankruptStock.Id));
	}

	private TradeOffer? GetPublicOffer(Guid stockId)
	{
		return _ctx.TradeOffer
			.Where(x => x.Type == OfferType.PublicOfferring && x.StockId == stockId)
			.FirstOrDefault();
	}

	[Test]
	public void RemoveAllOffersForStock_NullStock_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(
			() => _manager.RemoveAllOffersForStock(null));
	}

	[Test]
	public void RemoveAllOffersForStock_WrongStock_ShouldThrow()
	{
		Assert.Throws<KeyNotFoundException>(
			() => _manager.RemoveAllOffersForStock(Guid.NewGuid()));
	}

	[Test]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(2)]
	[TestCase(14)]
	public void RemoveAllOffersForStock_PositiveTest(int ownerships)
	{
		//Arange
		var stockId = AddStock().Id;
		for (int i = 0; i < ownerships; i++)
		{
			_ctx.Add(new TradeOffer
			{
				StockId = stockId
			});
		}
		_ctx.SaveChanges();
		Assert.AreEqual(ownerships > 0, _ctx.TradeOffer.Any());

		//Act
		_manager.RemoveAllOffersForStock(stockId);
		_ctx.SaveChanges();

		//Assert
		Assert.False(_ctx.TradeOffer.Any());
	}
}
