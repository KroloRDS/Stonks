﻿using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using Moq;
using MediatR;
using NUnit.Framework;

using Stonks.Models;
using Stonks.Providers;
using Stonks.Responses.Bankruptcy;
using Stonks.Requests.Queries.Bankruptcy;
using Stonks.Requests.Commands.Bankruptcy;

namespace UnitTests.Handlers.Commands.Bankruptcy;

public class BattleRoyaleRoundTest : InMemoryDb
{
	private readonly Mock<IMediator> _mediator = new();
	private readonly Mock<IStonksConfiguration> _config = new();
	private readonly BattleRoyaleRoundCommandHandler _handler;

	public BattleRoyaleRoundTest()
	{
		_handler = new BattleRoyaleRoundCommandHandler(
			_ctx, _mediator.Object, _config.Object);
	}

	[Test]
	public void BattleRoyaleRound_PositiveTest()
	{
		//Arrange
		_config.Setup(x => x.NewStocksAfterRound()).Returns(1);
		_mediator.Setup(x => x.Send(
			It.IsAny<GetWeakestStockIdQuery>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(new GetWeakestStockIdResponse(AddStock().Id));

		//Act
		_handler.BattleRoyaleRound(CancellationToken.None).Wait();

		//Assert
		VerifyMediatorMocks();
	}

	private void VerifyMediatorMocks()
	{
		_mediator.Verify(x => x.Send(It.IsAny<GetWeakestStockIdQuery>(),
			It.IsAny<CancellationToken>()), Times.Once());
		_mediator.VerifyNoOtherCalls();
		_config.Verify(x => x.NewStocksAfterRound(), Times.Once());
		_config.VerifyNoOtherCalls();
	}

	[Test]
	[TestCase(default)]
	[TestCase("8bc61de3-1cc0-45f7-81a4-75c5588c3f16")]
	public void Bankrupt_WrongStock_ShouldThrow(Guid id)
	{
		Assert.ThrowsAsync<KeyNotFoundException>(
			() => _handler.Bankrupt(id, CancellationToken.None));
	}

	[Test]
	public void Bankrupt_PositiveTest()
	{
		var stock = AddStock();
		_handler.Bankrupt(stock.Id, CancellationToken.None).Wait();
		Assert.IsTrue(stock.Bankrupt);
		Assert.Zero(stock.PublicallyOfferredAmount);
		Assert.NotNull(stock.BankruptDate);
	}

	[Test]
	[TestCase(default)]
	[TestCase("209a450e-142c-4d64-a073-2783862e0b64")]
	public void RemoveSharesAndOffers_WrongStock_ShouldThrow(Guid id)
	{
		Assert.ThrowsAsync<KeyNotFoundException>(
			() => _handler.RemoveSharesAndOffers(id, CancellationToken.None));
	}

	[Test]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(2)]
	[TestCase(14)]
	public void RemoveAllOffersForStock_PositiveTest(int count)
	{
		//Arange
		var stockId = AddStock().Id;
		var range = Enumerable.Range(1, count);
		var offers = range.Select(x => new TradeOffer { StockId = stockId });
		var shares = range.Select(x => new Share
		{
			StockId = stockId,
			OwnerId = Guid.NewGuid().ToString()
		});

		_ctx.AddRange(offers);
		_ctx.AddRange(shares);
		_ctx.SaveChanges();

		Assert.AreEqual(count > 0, _ctx.Share.Any());
		Assert.AreEqual(count > 0, _ctx.TradeOffer.Any());

		//Act
		_handler.RemoveSharesAndOffers(stockId, CancellationToken.None).Wait();
		_ctx.SaveChanges();

		//Assert
		Assert.False(_ctx.Share.Any());
		Assert.False(_ctx.TradeOffer.Any());
	}

	[Test]
	[TestCase(0)]
	[TestCase(-2)]
	[TestCase(-99)]
	public void UpdatePublicallyOfferedAmount_WrongAmount_ShouldThrow(int amount)
	{
		Assert.ThrowsAsync<ArgumentOutOfRangeException>(
			() => _handler.UpdatePublicallyOfferedAmount(
				amount, AddStock().Id, CancellationToken.None));
	}

	[Test]
	public void UpdatePublicallyOfferedAmount_PositiveTest()
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

		var stock1 = AddStock(0);
		var stock2 = AddStock(smallerAmount);
		var stock3 = AddStock(biggerAmount);
		var excludedStock = AddStock(0);
		var bankruptStock = AddBankruptStock();

		//Act
		_handler.UpdatePublicallyOfferedAmount(amount,
			excludedStock.Id, CancellationToken.None).Wait();
		_ctx.SaveChanges();

		//Assert
		Assert.AreEqual(amount, GetPublicAmount(stock1.Id));
		Assert.AreEqual(amount, GetPublicAmount(stock2.Id));
		Assert.AreEqual(biggerAmount, GetPublicAmount(stock3.Id));
		Assert.Zero(GetPublicAmount(excludedStock.Id));
		Assert.Zero(GetPublicAmount(bankruptStock.Id));
	}

	private int GetPublicAmount(Guid stockId)
	{
		return _ctx.GetById<Stock>(stockId).PublicallyOfferredAmount;
	}
}
