﻿using MediatR;
using Stonks.Data;
using Stonks.Models;
using Stonks.CustomExceptions;

namespace Stonks.Requests.Commands.Trade.CancelOffer;

public record CancelOfferCommand(Guid OfferId) : IRequest;

public class CancelOfferCommandHandler : IRequestHandler<CancelOfferCommand>
{
    private readonly AppDbContext _ctx;

    public CancelOfferCommandHandler(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<Unit> Handle(CancelOfferCommand request,
        CancellationToken cancellationToken)
    {
        var offer = await ValidateRequest(request);
        await Task.Run(() => _ctx.TradeOffer.Remove(offer), cancellationToken);
        return Unit.Value;
    }

    private async Task<TradeOffer> ValidateRequest(CancelOfferCommand request)
    {
        var offer = await _ctx.GetById<TradeOffer>(request.OfferId);

        if (offer.Type is OfferType.PublicOfferring)
            throw new PublicOfferingException();

        return offer;
    }
}
