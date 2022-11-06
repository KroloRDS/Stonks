using MediatR;
using Microsoft.EntityFrameworkCore;

using Stonks.Util;
using Stonks.Data;
using Stonks.Data.Models;

namespace Stonks.CQRS.Commands.Trade;

public record PlaceOfferCommand(
    Guid StockId,
    Guid WriterId,
    int Amount,
    OfferType Type,
    decimal Price
) : IRequest;

public class PlaceOfferCommandHandler : BaseCommand<PlaceOfferCommand>
{
    public PlaceOfferCommandHandler(AppDbContext ctx, IMediator mediator)
		: base(ctx, mediator) {}

    public override async Task<Unit> Handle(PlaceOfferCommand command,
        CancellationToken cancellationToken)
    {
        var request = await ValidateRequest(command, cancellationToken);

        // Try to match with existin offers first
        var offers = request.OfferType == OfferType.Buy ?
            FindSellOffers(request.StockId, request.Price) :
            FindBuyOffers(request.StockId, request.Price);

        foreach (var offer in offers)
        {
            if (offer.Amount < request.Amount)
            {
                await _mediator.Send(new AcceptOfferCommand(request.WriterId,
                    offer.Id), cancellationToken);
                request.Amount -= offer.Amount;
            }
            else
            {
                await _mediator.Send(new AcceptOfferCommand(request.WriterId,
                    offer.Id, request.Amount), cancellationToken);
                return Unit.Value;
            }
        }

        // If there are still stocks to sell / buy
        var newOffer = new TradeOffer
        {
            Amount = request.Amount,
            StockId = request.StockId,
            WriterId = request.WriterId,
            Type = request.OfferType,
            Price = request.Price
        };

        await _ctx.AddAsync(newOffer, cancellationToken);
        await _ctx.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task<ValidatedRequest> ValidateRequest(
        PlaceOfferCommand command, CancellationToken cancellationToken)
    {
        if (command.Type == OfferType.PublicOfferring)
            throw new PublicOfferingException();

        var stock = await _ctx.GetById<Stock>(command.StockId);
        if (stock.Bankrupt)
            throw new BankruptStockException();

        var amount = command.Amount.AssertPositive();
        var price = command.Price.AssertPositive();
        var writerId = await _ctx.EnsureExist<User>(
            command.WriterId, cancellationToken);

        if (command.Type == OfferType.Sell)
        {
            await CheckOwnedShares(writerId, stock.Id,
                amount, cancellationToken);
        }
        if (command.Type == OfferType.Buy)
        {
            await CheckAvailableFunds(command.Price * command.Amount,
                writerId, cancellationToken);
        }

        return new ValidatedRequest(command.Type, amount,
            stock.Id, price, writerId);
    }

    private async Task CheckOwnedShares(Guid writerId, Guid stockId,
        int amount, CancellationToken cancellationToken)
    {
        var share = await _ctx.GetShares(writerId,
            stockId, cancellationToken);
        if (share?.Amount is null || share.Amount < amount)
            throw new NoStocksOnSellerException();
    }

    private async Task CheckAvailableFunds(decimal amount,
        Guid userId, CancellationToken cancellationToken)
    {
        var inOtherOffers = _ctx.TradeOffer
            .Where(x => x.Type == OfferType.Buy && x.WriterId == userId)
            .SumAsync(x => x.Price * x.Amount, cancellationToken);

        var user = _ctx.GetById<User>(userId);

        if ((await user).Funds < await inOtherOffers + amount)
            throw new InsufficientFundsException();
    }

    private IEnumerable<TradeOffer> FindBuyOffers(Guid stockId, decimal price)
    {
        return _ctx.TradeOffer.Where(x =>
           x.Type == OfferType.Buy &&
           x.StockId == stockId &&
           x.Price >= price)
            .OrderByDescending(x => x.Price)
            .ToList();
    }

    private IEnumerable<TradeOffer> FindSellOffers(Guid stockId, decimal price)
    {
        return _ctx.TradeOffer.Where(x =>
            x.Type != OfferType.Buy &&
            x.StockId == stockId &&
            x.Price <= price)
            .OrderBy(x => x.Price)
            .ToList();
    }

    private record ValidatedRequest(OfferType OfferType, int Amount,
        Guid StockId, decimal Price, Guid WriterId)
    {
        public int Amount { get; set; } = Amount;
    }
}
