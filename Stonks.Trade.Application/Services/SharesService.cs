using Stonks.Common.Utils;
using Stonks.Trade.Domain.Models;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Application.Services;

public interface ISharesService
{
	Task Transfer(Guid userId, TradeOffer offer,
		int amount, CancellationToken cancellationToken = default);
}

public class SharesService : ISharesService
{
	private readonly ICurrentTime _currentTime;
	private readonly IShareRepository _shares;
	private readonly ITransactionRepository _transaction;

	public SharesService(IShareRepository shares, ICurrentTime currentTime,
		ITransactionRepository transaction)
	{
		_currentTime = currentTime;
		_shares = shares;
		_transaction = transaction;
	}

	public async Task Transfer(Guid userId, TradeOffer offer,
		int amount, CancellationToken cancellationToken = default)
	{
		Guid writerId;
		if (offer.Type != OfferType.PublicOfferring && offer.WriterId is null)
			throw new ArgumentNullException(nameof(offer.WriterId));
		else writerId = offer.WriterId!.Value;

		await Task.WhenAll(
			ProcessOfferSide(writerId, offer, amount, cancellationToken),
			ProcessClientSide(userId, offer, amount, cancellationToken),
			AddTransactionLog(userId, writerId, offer, amount, cancellationToken));
	}

	private async Task ProcessOfferSide(Guid writerId, TradeOffer offer,
		int amount, CancellationToken cancellationToken = default)
	{
		if (offer.Type == OfferType.Buy)
			await _shares.GiveSharesToUser(offer.StockId,
				writerId, amount, cancellationToken);

		if (offer.Type == OfferType.Sell)
			await _shares.TakeSharesFromUser(offer.StockId,
				writerId, amount, cancellationToken);
	}

	private async Task ProcessClientSide(Guid userId, TradeOffer offer,
		int amount, CancellationToken cancellationToken = default)
	{
		if (offer.Type == OfferType.Buy)
			await _shares.TakeSharesFromUser(offer.StockId,
				userId, amount, cancellationToken);

		if (offer.Type == OfferType.Sell)
			await _shares.GiveSharesToUser(offer.StockId,
				userId, amount, cancellationToken);
	}

	private async Task AddTransactionLog(Guid clientId,
		Guid writerId, TradeOffer offer, int amount,
		CancellationToken cancellationToken = default)
	{
		Guid? sellerId = offer.Type switch
		{
			OfferType.Buy => clientId,
			OfferType.Sell => writerId,
			OfferType.PublicOfferring => null,
			_ => throw new ArgumentOutOfRangeException(nameof(offer.Type)),
		};

		await _transaction.AddLog(new Transaction
		{
			StockId = offer.StockId,
			BuyerId = offer.Type == OfferType.Buy ? writerId : clientId,
			SellerId = sellerId,
			Amount = amount,
			Price = offer.Price,
			Timestamp = _currentTime.Get()
		}, cancellationToken);
	}
}
