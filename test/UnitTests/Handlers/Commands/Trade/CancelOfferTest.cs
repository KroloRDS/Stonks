using System;
using System.Linq;
using System.Collections.Generic;
using MediatR;
using NUnit.Framework;
using Stonks.Models;
using Stonks.Helpers;
using Stonks.Requests.Commands.Trade;


namespace UnitTests.Handlers.Commands.Trade;

public class CancelOfferTest : CommandTest<CancelOfferCommand>
{
	protected override IRequestHandler<CancelOfferCommand, Unit> GetHandler()
	{
		return new CancelOfferCommandHandler(_ctx);
	}

	[Test]
	public void CancelOffer_WrongOffer_ShouldThrow()
	{
		AssertThrows<KeyNotFoundException>(
			new CancelOfferCommand(default));
		AssertThrows<KeyNotFoundException>(
			new CancelOfferCommand(Guid.NewGuid()));
	}

	[Test]
	public void CancelOffer_PublicOffering_ShouldThrow()
	{
		var offer = AddOffer(OfferType.PublicOfferring);
		AssertThrows<PublicOfferingException>(
			new CancelOfferCommand(offer.Id));
	}

	[Test]
	[TestCase(OfferType.Buy)]
	[TestCase(OfferType.Sell)]
	public void CancelOffer_PositiveTest(OfferType type)
	{
		var offer = AddOffer(type);
		Handle(new CancelOfferCommand(offer.Id));
		Assert.False(_ctx.TradeOffer.Any());
	}
}
