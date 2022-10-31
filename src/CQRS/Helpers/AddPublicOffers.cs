﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Stonks.Data;
using Stonks.CQRS.Queries.Common;
using Stonks.Data.Models;

namespace Stonks.CQRS.Helpers;

public interface IAddPublicOffers
{
    Task Handle(int amount, Guid bankruptedId,
        CancellationToken cancellationToken);
}

public class AddPublicOffers : IAddPublicOffers
{
    private readonly AppDbContext _ctx;
    private readonly IMediator _mediator;

    public AddPublicOffers(AppDbContext ctx, IMediator mediator)
    {
        _ctx = ctx;
        _mediator = mediator;
    }

    public async Task Handle(int amount, Guid bankruptedId,
        CancellationToken cancellationToken)
    {
        var ids = _ctx.Stock
            .Where(x => !x.Bankrupt && x.Id != bankruptedId)
            .Select(x => x.Id);

        foreach (var id in ids)
        {
            await AddPublicOffer(id, amount, cancellationToken);
        }
    }

    private async Task AddPublicOffer(Guid stockId, int amount,
        CancellationToken cancellationToken)
    {
        var offer = await _ctx.TradeOffer.FirstOrDefaultAsync(x =>
            x.Type == OfferType.PublicOfferring &&
            x.StockId == stockId,
            cancellationToken);

        if (offer == default)
            await CreateNewPublicOffer(stockId, amount, cancellationToken);
        else if (offer.Amount < amount)
            offer.Amount = amount;
    }

    private async Task CreateNewPublicOffer(Guid stockId, int amount,
        CancellationToken cancellationToken)
    {
        var avgPrice = await _mediator.Send(
            new GetCurrentPriceQuery(stockId), cancellationToken);

        await _ctx.AddAsync(new TradeOffer
        {
            Amount = amount,
            Price = avgPrice.Price,
            StockId = stockId,
            Type = OfferType.PublicOfferring
        }, cancellationToken);
    }
}
