using Stonks.Common.Utils;
using Stonks.Trade.Domain.Models;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Application.Services;

public interface IShareService
{
	Task Transfer(Guid userId, TradeOffer offer,
		int amount, CancellationToken cancellationToken = default);
}

public class ShareService : IShareService
{
	private readonly ICurrentTime _currentTime;
	private readonly IShareRepository _share;
	private readonly ITransactionRepository _transaction;

	public ShareService(ICurrentTime currentTime, IShareRepository share,
		ITransactionRepository transaction)
	{
		_currentTime = currentTime;
		_share = share;
		_transaction = transaction;
	}

	public async Task Transfer(Guid userId, TradeOffer offer,
		int amount, CancellationToken cancellationToken = default)
	{
		if (offer.Type != OfferType.PublicOfferring)
		{
			var writerId = offer?.WriterId ??
				throw new ArgumentNullException(nameof(offer.WriterId));

			await ProcessOfferSide(writerId, offer, amount, cancellationToken);
		}

		await Task.WhenAll(
			ProcessClientSide(userId, offer, amount, cancellationToken),
			AddTransactionLog(userId, offer.WriterId,
				offer, amount, cancellationToken));
	}

	private async Task ProcessOfferSide(Guid writerId, TradeOffer offer,
		int amount, CancellationToken cancellationToken = default)
	{
		if (offer.Type == OfferType.Sell)
			await _share.TakeSharesFromUser(offer.StockId,
				writerId, amount, cancellationToken);

		if (offer.Type == OfferType.Buy)
			await _share.GiveSharesToUser(offer.StockId,
				writerId, amount, cancellationToken);
	}

	private async Task ProcessClientSide(Guid userId, TradeOffer offer,
		int amount, CancellationToken cancellationToken = default)
	{
		if (offer.Type == OfferType.Buy)
			await _share.TakeSharesFromUser(offer.StockId,
				userId, amount, cancellationToken);
		else
			await _share.GiveSharesToUser(offer.StockId,
				userId, amount, cancellationToken);
	}

	private async Task AddTransactionLog(Guid clientId,
		Guid? writerId, TradeOffer offer, int amount,
		CancellationToken cancellationToken = default)
	{
		Guid? sellerId = offer.Type switch
		{
			OfferType.Buy => clientId,
			OfferType.Sell => writerId,
			OfferType.PublicOfferring => null,
			_ => throw new ArgumentOutOfRangeException(nameof(offer.Type)),
		};
		var buyerId = offer.Type != OfferType.Buy ? clientId :
			writerId ?? throw new ArgumentNullException(nameof(writerId));

		await _transaction.AddLog(new Transaction
		{
			StockId = offer.StockId,
			BuyerId = buyerId,
			SellerId = sellerId,
			Amount = amount,
			Price = offer.Price,
			Timestamp = _currentTime.Get()
		}, cancellationToken);
	}
}
