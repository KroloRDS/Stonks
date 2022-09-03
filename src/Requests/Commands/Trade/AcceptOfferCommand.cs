using MediatR;
using Stonks.Data;
using Stonks.Helpers;
using Stonks.Models;

namespace Stonks.Requests.Commands.Trade;

public record AcceptOfferCommand(Guid UserId, Guid OfferId, int? Amount)
	: IRequest;

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
		var offer = _ctx.GetById<TradeOffer>(request.OfferId);
		var amount = request.Amount ?? offer.Amount;
		amount.AssertPositive();

		var buyShares = BuyShares(request.UserId, offer,
			amount, cancellationToken);
		var settleMoney = SettleMoney(request.UserId, offer,
			amount, cancellationToken);

		offer.Amount -= amount;
		if (offer.Amount <= 0)
			_ctx.TradeOffer.Remove(offer);

		await Task.WhenAll(buyShares, settleMoney);
		await _ctx.SaveChangesAsync(cancellationToken);
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
			buyerId = Guid.Parse(offer.WriterId!);
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
				sellerId = Guid.Parse(offer.WriterId!);
			}
		}

		await _mediator.Send(new TransferSharesCommand(
			offer.StockId,
			buyerId,
			sellerId,
			amount,
			buyFromUser), cancellationToken);
	}

	private async Task TransferMoney(string? payerId, Guid recipientId,
		decimal amount, CancellationToken cancellationToken)
	{
		if (payerId is null)
			throw new ArgumentNullException(payerId);

		await TransferMoney(Guid.Parse(payerId), recipientId,
			amount, cancellationToken);
	}

	private async Task TransferMoney(Guid payerId, string? recipientId,
		decimal amount, CancellationToken cancellationToken)
	{
		if (recipientId is null)
			throw new ArgumentNullException(recipientId);

		await TransferMoney(payerId, Guid.Parse(recipientId),
			amount, cancellationToken);
	}

	private async Task TransferMoney(Guid payerId, Guid recipientId,
		decimal amount, CancellationToken cancellationToken)
	{
		var take = _mediator.Send(new TakeMoneyCommand(
					payerId, amount), cancellationToken);
		var give = _mediator.Send(new GiveMoneyCommand(
					recipientId, amount), cancellationToken);
		await Task.WhenAll(give, take);
	}
}
