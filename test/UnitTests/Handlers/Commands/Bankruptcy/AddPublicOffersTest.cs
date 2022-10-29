using System;
using System.Linq;
using System.Threading;

using Moq;
using MediatR;
using NUnit.Framework;

using Stonks.Models;
using Stonks.Responses.Common;
using Stonks.Requests.Queries.Common;
using Stonks.Requests.Commands.Bankruptcy;

namespace UnitTests.Handlers.Commands.Bankruptcy;

public class AddPublicOffersTest : InMemoryDb
{
	private readonly Mock<IMediator> _mediator = new();
	private readonly AddPublicOffersHandler _handler;

	public AddPublicOffersTest()
	{
		_mediator.Setup(x => x.Send(
			It.IsAny<GetCurrentPriceQuery>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(new GetCurrentPriceResponse(0));
		_handler = new AddPublicOffersHandler(_ctx, _mediator.Object);
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
		var excludedStock = AddStock();
		var bankruptStock = AddBankruptStock();

		_ctx.AddRange(new[]
		{
			new TradeOffer
			{
				Amount = smallerAmount,
				StockId = stock2.Id,
				Type = OfferType.PublicOfferring
			},
			new TradeOffer
			{
				Amount = biggerAmount,
				StockId = stock3.Id,
				Type = OfferType.PublicOfferring
			}
		});
		_ctx.SaveChanges();

		//Act
		_handler.Handle(amount, excludedStock.Id, 
			CancellationToken.None).Wait();
		_ctx.SaveChanges();

		//Assert
		_mediator.Verify(x => x.Send(It.IsAny<GetCurrentPriceQuery>(),
			It.IsAny<CancellationToken>()), Times.Once());
		_mediator.VerifyNoOtherCalls();

		Assert.AreEqual(amount, GetPublicOffer(stock1.Id)?.Amount);
		Assert.AreEqual(amount, GetPublicOffer(stock2.Id)?.Amount);
		Assert.AreEqual(biggerAmount, GetPublicOffer(stock3.Id)?.Amount);
		Assert.Null(GetPublicOffer(excludedStock.Id));
		Assert.Null(GetPublicOffer(bankruptStock.Id));
	}

	private TradeOffer? GetPublicOffer(Guid stockId)
	{
		return _ctx.TradeOffer
			.SingleOrDefault(x => x.StockId == stockId &&
				x.Type == OfferType.PublicOfferring);
	}
}
