using Stonks.Trade.Domain.Models;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Application.Helpers;

public interface IOfferHelper
{
	Task Accept(Guid userId, Guid offerId, int amount,
		CancellationToken cancellationToken = default);
}

public class OfferHelper : IOfferHelper
{
	private readonly IUserRepository _user;
	private readonly IOfferRepository _offer;
	private readonly ISharesHelper _shares;

	public OfferHelper(IUserRepository user,
		IOfferRepository offer, ISharesHelper shares)
	{
		_user = user;
		_offer = offer;
		_shares = shares;
	}

	public async Task Accept(Guid userId, Guid offerId, int amount,
		CancellationToken cancellationToken = default)
	{
		var offer = await _offer.Get(offerId);
		if (amount > offer!.Amount) amount = offer.Amount;

		await _shares.Transfer(userId, offer, amount, cancellationToken);
		await SettleMoney(userId, offer, amount);

		offer.Amount -= amount;
		if (offer.Amount <= 0) _offer.Cancel(offerId);
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
			OfferType.PublicOfferring =>
				_user.ChangeBalance(clientId, -amount),
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
