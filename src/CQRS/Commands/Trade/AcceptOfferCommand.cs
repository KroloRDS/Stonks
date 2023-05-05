using MediatR;
using Stonks.Util;
using Stonks.Data;
using Stonks.Data.Models;
using Stonks.CQRS.Helpers;

namespace Stonks.CQRS.Commands.Trade;

public record AcceptOfferCommand(Guid UserId,
    Guid OfferId, int? Amount = null) : IRequest<Unit>;

public class AcceptOfferCommandHandler : BaseCommand<AcceptOfferCommand>
{
	private readonly ITransferShares _transferShares;
	private readonly IGiveMoney _giveMoney;

	public AcceptOfferCommandHandler(AppDbContext ctx,
		ITransferShares transferShares, IGiveMoney giveMoney) : base(ctx)
    {
		_transferShares = transferShares;
		_giveMoney = giveMoney;
	}

    public override async Task<Unit> Handle(AcceptOfferCommand request,
        CancellationToken cancellationToken)
    {
        var task = AcceptOffer(request, cancellationToken);
        await _ctx.ExecuteTransaction(task,
            nameof(AcceptOfferCommandHandler), cancellationToken);
        return Unit.Value;
    }

	private async Task AcceptOffer(AcceptOfferCommand request,
		CancellationToken cancellationToken)
	{
		var offer = await _ctx.GetById<TradeOffer>(request.OfferId);
		var amount = request.Amount ?? offer.Amount;
		amount.AssertPositive();
		if (amount > offer.Amount) amount = offer.Amount;

		await TransferShares(request.UserId, offer, amount, cancellationToken);
		await SettleMoney(request.UserId, offer, amount);

		offer.Amount -= amount;
		if (offer.Amount <= 0) _ctx.TradeOffer.Remove(offer);
	}

	private async Task TransferShares(Guid userId, TradeOffer offer,
		int amount, CancellationToken cancellationToken)
	{
		await _transferShares.Handle(new TransferSharesCommand
		{
			StockId = offer.StockId,
			Amount = amount,
			BuyerId = offer.Type == OfferType.Buy ?
				offer.WriterId!.Value : userId,
			BuyFromUser = offer.Type != OfferType.PublicOfferring,
			SellerId = offer.Type switch
			{
				OfferType.Buy => userId,
				OfferType.Sell => offer.WriterId!.Value,
				_ => null
			}
		}, cancellationToken);
	}

	private async Task SettleMoney(Guid clientId, TradeOffer offer, int amount)
	{
		var offerValue = offer.Price * amount;
		var task = offer.Type switch
		{
			OfferType.Sell => TransferMoney(clientId,
				offer.WriterId, offerValue),
			OfferType.Buy => TransferMoney(offer.WriterId,
				clientId, offerValue),
			OfferType.PublicOfferring => TakeMoney(clientId, offerValue),
			_ => throw new ArgumentOutOfRangeException(nameof(offer.Type))
		};
		await task;
	}

	private async Task TransferMoney(Guid? payerId,
		Guid? recipientId, decimal amount)
	{
		if (payerId is null)
			throw new ArgumentNullException(nameof(payerId));

		if (recipientId is null)
			throw new ArgumentNullException(nameof(recipientId));

		await TakeMoney(payerId.Value, amount);
		await _giveMoney.Handle(recipientId.Value, amount);
	}

	public async Task TakeMoney(Guid userId, decimal amount)
	{
		amount.AssertPositive();
		var user = await _ctx.GetById<User>(userId);
		user.Funds -= amount;
		if (user.Funds < 0) throw new InsufficientFundsException();
	}
}
