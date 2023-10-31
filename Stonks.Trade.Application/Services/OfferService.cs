using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Application.Services;

public interface IOfferService
{
	Task Accept(Guid userId, Guid offerId, int amount,
		CancellationToken cancellationToken = default);
}

public class OfferService : IOfferService
{
	private readonly IOfferRepository _offer;
	private readonly IShareService _share;
	private readonly IUserService _user;

	public OfferService(IOfferRepository offer,
		IShareService share, IUserService user)
	{
		_offer = offer;
		_share = share;
		_user = user;
	}

	public async Task Accept(Guid userId, Guid offerId, int amount,
		CancellationToken cancellationToken = default)
	{
		if (amount < 1)
			throw new ArgumentOutOfRangeException(nameof(amount));

		var offer = await _offer.Get(offerId, cancellationToken) ??
			throw new KeyNotFoundException($"Offer: {offerId}");

		if (amount > offer.Amount) amount = offer.Amount;

		await _share.Transfer(userId, offer, amount, cancellationToken);
		await _user.SettleMoney(userId, offer, amount);

		if (amount == offer.Amount)
			_offer.Cancel(offerId);
		else
			await _offer.DecreaseOfferAmount(offerId, amount);
	}
}
