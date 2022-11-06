using System;
using System.Linq;
using System.Collections.Generic;
using MediatR;
using NUnit.Framework;

using Stonks.Util;
using Stonks.Data.Models;
using Stonks.CQRS.Commands.Trade;

namespace UnitTests.CQRS.Commands.Trade;

public class CancelOfferTest : CommandTest<CancelOfferCommand>
{
    protected override IRequestHandler<CancelOfferCommand, Unit> GetHandler()
    {
        return new CancelOfferCommandHandler(_ctx);
    }

	[Test]
	[TestCase(default)]
	[TestCase(_zeroGuid)]
	[TestCase(_randomGuid)]
	public void CancelOffer_WrongOffer_ShouldThrow(Guid id)
    {
        AssertThrows<KeyNotFoundException>(new CancelOfferCommand(id));
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
