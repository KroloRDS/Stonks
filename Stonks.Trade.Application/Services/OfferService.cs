using Stonks.Trade.Domain.Models;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Application.Services;

public interface IOfferService
{
	Task Accept(Guid userId, Guid offerId, int amount,
		CancellationToken cancellationToken = default);
}

public class OfferService : IOfferService
{
	private readonly IUserRepository _user;
	private readonly IOfferRepository _offer;
	private readonly ISharesService _shares;

	public OfferService(IUserRepository user,
		IOfferRepository offer, ISharesService shares)
	{
		_user = user;
		_offer = offer;
		_shares = shares;
	}

	public async Task Accept(Guid userId, Guid offerId, int amount,
		CancellationToken cancellationToken = default)
	{
		if (amount < 1)
			throw new ArgumentOutOfRangeException(nameof(amount));

		var offer = await _offer.Get(offerId, cancellationToken) ??
			throw new KeyNotFoundException($"Offer: {offerId}");

		if (amount > offer.Amount) amount = offer.Amount;

		await _shares.Transfer(userId, offer, amount, cancellationToken);
		await SettleMoney(userId, offer, amount);

		if (amount == offer.Amount)
			_offer.Cancel(offerId);
		else
			await _offer.DecreaseOfferAmount(offerId, amount);
	}

	public async Task SettleMoney(Guid clientId, TradeOffer offer, int amount)
	{
		var offerValue = offer.Price * amount;
		var task = offer.Type switch
		{
			OfferType.Sell => TransferMoney(clientId,
				offer.WriterId, offerValue),
			OfferType.Buy => TransferMoney(offer.WriterId,
				clientId, offerValue),
			OfferType.PublicOfferring =>
				_user.ChangeBalance(clientId, -offerValue),
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

		await Task.WhenAll(
			_user.ChangeBalance(payerId.Value, -amount),
			_user.ChangeBalance(recipientId.Value, amount));
	}
}
