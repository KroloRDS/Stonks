using Stonks.Trade.Domain.Models;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Application.Services;

public interface IUserService
{
	Task SettleMoney(Guid clientId, TradeOffer offer, int amount);
}

public class UserService : IUserService
{
	private readonly IUserRepository _user;

	public UserService(IUserRepository user)
	{
		_user = user;
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
