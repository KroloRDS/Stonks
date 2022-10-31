using MediatR;
using Stonks.Util;
using Stonks.Data;
using Stonks.Data.Models;
using Stonks.CQRS.Helpers;

namespace Stonks.CQRS.Commands.Trade;

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
        await _ctx.ExecuteTransaction(task,
            nameof(AcceptOfferCommandHandler), cancellationToken);
        return Unit.Value;
    }

    public async Task AcceptOffer(AcceptOfferCommand request,
        CancellationToken cancellationToken)
    {
        var offer = await _ctx.GetById<TradeOffer>(request.OfferId);
        var amount = request.Amount ?? offer.Amount;
        amount.AssertPositive();

        await TransferShares(request.UserId, offer, amount, cancellationToken);
        await SettleMoney(request.UserId, offer, amount, cancellationToken);

        offer.Amount -= amount;
        if (offer.Amount <= 0) _ctx.TradeOffer.Remove(offer);
    }

	private async Task TransferShares(Guid userId, TradeOffer offer,
		int amount, CancellationToken cancellationToken)
	{
		var command = new TransferSharesCommand
		{
			StockId = offer.StockId,
			Amount = amount,
			BuyerId = offer.Type == OfferType.Buy ?
				offer.WriterId!.Value : userId,
			BuyFromUser = offer.Type == OfferType.PublicOfferring,
			SellerId = offer.Type switch
			{
				OfferType.Buy => userId,
				OfferType.Sell => offer.WriterId!.Value,
				_ => null
			}
		};

		TranferShares
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
            OfferType.PublicOfferring => TakeMoney(clientId, offerValue),
            _ => throw new ArgumentOutOfRangeException(nameof(offer.Type))
        };
        await task;
    }

    private async Task TransferMoney(Guid? payerId, Guid? recipientId,
        decimal amount, CancellationToken cancellationToken)
    {
        if (payerId is null)
            throw new ArgumentNullException(nameof(payerId));

        if (recipientId is null)
            throw new ArgumentNullException(nameof(recipientId));

        await TakeMoney(payerId.Value, amount);
        await _mediator.Send(new GiveMoneyCommand(
            recipientId.Value, amount), cancellationToken);
    }

	public async Task TakeMoney(Guid userId, decimal amount)
	{
		amount.AssertPositive();
		var user = await _ctx.GetById<User>(userId);
		user.Funds -= amount;
		if (user.Funds < 0) throw new InsufficientFundsException();
	}
}
