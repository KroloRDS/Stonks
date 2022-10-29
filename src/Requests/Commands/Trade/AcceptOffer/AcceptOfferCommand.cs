using MediatR;
using Stonks.Data;
using Stonks.Models;
using Stonks.ExtensionMethods;

namespace Stonks.Requests.Commands.Trade.AcceptOffer;

public record AcceptOfferCommand(Guid UserId,
    Guid OfferId, int? Amount = null) : IRequest;

public class AcceptOfferCommandHandler : IRequestHandler<AcceptOfferCommand>
{
    private readonly AppDbContext _ctx;
    private readonly IMediator _mediator;

    public AcceptOfferCommandHandler(AppDbContext ctx,
        IMediator mediator)
    {
        _ctx = ctx;
        _mediator = mediator;
    }

    public async Task<Unit> Handle(AcceptOfferCommand request,
        CancellationToken cancellationToken)
    {
        var task = AcceptOffer(request, cancellationToken);
        await _ctx.ExecuteTransactionAsync(task,
            nameof(AcceptOfferCommandHandler), cancellationToken);
        return Unit.Value;
    }

    public async Task AcceptOffer(AcceptOfferCommand request,
        CancellationToken cancellationToken)
    {
        var offer = await _ctx.GetById<TradeOffer>(request.OfferId);
        var amount = request.Amount ?? offer.Amount;
        amount.AssertPositive();

        await BuyShares(request.UserId, offer, amount, cancellationToken);
        await SettleMoney(request.UserId, offer, amount, cancellationToken);

        offer.Amount -= amount;
        if (offer.Amount <= 0)
            _ctx.TradeOffer.Remove(offer);
    }

    private async Task SettleMoney(Guid clientId, TradeOffer offer,
        int amount, CancellationToken cancellationToken)
    {
        var offerValue = offer.Price * amount;
        var task = offer.Type switch
        {
            OfferType.Sell => TransferMoney(clientId, offer.WriterId,
                offerValue, cancellationToken),
            OfferType.Buy => TransferMoney(offer.WriterId, clientId,
                offerValue, cancellationToken),
            OfferType.PublicOfferring => _mediator.Send(new TakeMoneyCommand(
                clientId, offerValue), cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(offer.Type))
        };
        await task;
    }

    private async Task BuyShares(Guid userId, TradeOffer offer,
        int amount, CancellationToken cancellationToken)
    {
        Guid buyerId;
        Guid? sellerId = null;
        var buyFromUser = true;

        if (offer.Type == OfferType.Buy)
        {
            buyerId = offer.WriterId!.Value;
            sellerId = userId;
        }
        else
        {
            buyerId = userId;
            if (offer.Type == OfferType.PublicOfferring)
            {
                buyFromUser = false;
            }
            else
            {
                sellerId = offer.WriterId!.Value;
            }
        }

        await _mediator.Send(new TransferSharesCommand(
            offer.StockId,
            amount,
            buyerId,
            buyFromUser,
            sellerId), cancellationToken);
    }

    private async Task TransferMoney(Guid? payerId, Guid? recipientId,
        decimal amount, CancellationToken cancellationToken)
    {
		if (payerId is null)
			throw new ArgumentNullException(nameof(payerId));

		if (recipientId is null)
			throw new ArgumentNullException(nameof(recipientId));

		await _mediator.Send(new TakeMoneyCommand(
			payerId.Value, amount), cancellationToken);
        await _mediator.Send(new GiveMoneyCommand(
			recipientId.Value, amount), cancellationToken);
    }
}
