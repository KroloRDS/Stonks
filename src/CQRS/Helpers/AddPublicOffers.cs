using Microsoft.EntityFrameworkCore;
using Stonks.Data;
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

    public AddPublicOffers(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task Handle(int amount, Guid bankruptedId,
        CancellationToken cancellationToken)
    {
		var stocks = _ctx.Stock.Where(x => !x.Bankrupt && x.Id != bankruptedId);
        foreach (var stock in stocks)
        {
            await AddPublicOffer(stock, amount, cancellationToken);
        }
    }

    private async Task AddPublicOffer(Stock stock, int amount,
        CancellationToken cancellationToken)
    {
        var offer = await _ctx.TradeOffer.FirstOrDefaultAsync(x =>
            x.Type == OfferType.PublicOfferring &&
            x.StockId == stock.Id,
            cancellationToken);

        if (offer == default)
            await CreateNewPublicOffer(stock, amount, cancellationToken);
        else if (offer.Amount < amount)
            offer.Amount = amount;
    }

    private async Task CreateNewPublicOffer(Stock stock, int amount,
        CancellationToken cancellationToken)
    {
        await _ctx.AddAsync(new TradeOffer
        {
            Amount = amount,
            Price = stock.Price,
            StockId = stock.Id,
            Type = OfferType.PublicOfferring
        }, cancellationToken);
    }
}
