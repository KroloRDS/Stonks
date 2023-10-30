using AutoMapper;
using Stonks.Trade.Domain.Models;
using Stonks.Trade.Domain.Repositories;
using Stonks.Common.Db;
using EF = Stonks.Common.Db.EntityFrameworkModels;
using CommonRepositories = Stonks.Common.Db.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Stonks.Trade.Db.Repositories;

public class OfferRepository : IOfferRepository
{
	private readonly IMapper _mapper;
	private readonly AppDbContext _writeCtx;
	private readonly ReadOnlyDbContext _readCtx;
	private readonly CommonRepositories.IOfferRepository _offer;

	public OfferRepository(IMapper mapper, AppDbContext writeCtx,
		ReadOnlyDbContext readCtx, CommonRepositories.IOfferRepository offer)
	{
		_mapper = mapper;
		_writeCtx = writeCtx;
		_readCtx = readCtx;
		_offer = offer;
	}

	public async Task<int> PublicallyOfferdAmount(Guid stockId,
		CancellationToken cancellationToken = default) =>
		await _offer.PublicallyOfferdAmount(stockId, cancellationToken);

	public async Task<TradeOffer?> Get(Guid offerId,
		CancellationToken cancellationToken = default)
	{
		var offer = await _readCtx.GetById<EF.TradeOffer>(offerId);
		return offer is null ? null : _mapper.Map<TradeOffer>(offer);
	}

	public async Task<IEnumerable<TradeOffer>> GetUserBuyOffers(
		Guid userId, CancellationToken cancellationToken = default)
	{
		var offers = await _readCtx.TradeOffer.Where(x =>
		   x.Type == EF.OfferType.Buy &&
		   x.WriterId == userId)
			.ToListAsync(cancellationToken);

		return offers.Select(_mapper.Map<TradeOffer>);
	}

	public async Task<IEnumerable<TradeOffer>> GetUserSellOffers(
		Guid userId, CancellationToken cancellationToken = default)
	{
		var offers = await _readCtx.TradeOffer.Where(x =>
		   x.Type == EF.OfferType.Sell &&
		   x.WriterId == userId)
			.ToListAsync(cancellationToken);

		return offers.Select(_mapper.Map<TradeOffer>);
	}

	public async Task<IEnumerable<TradeOffer>> FindBuyOffers(Guid stockId,
		decimal price, CancellationToken cancellationToken = default)
	{
		var offers = await _readCtx.TradeOffer.Where(x =>
		   x.Type == EF.OfferType.Buy &&
		   x.StockId == stockId &&
		   x.Price >= price)
			.OrderByDescending(x => x.Price)
			.ToListAsync(cancellationToken);

		return offers.Select(_mapper.Map<TradeOffer>);
	}

	public async Task<IEnumerable<TradeOffer>> FindSellOffers(Guid stockId,
		decimal price, CancellationToken cancellationToken = default)
	{
		var offers = await _readCtx.TradeOffer.Where(x =>
			x.Type != EF.OfferType.Buy &&
			x.StockId == stockId &&
			x.Price <= price)
			.OrderBy(x => x.Price)
			.ToListAsync(cancellationToken);

		return offers.Select(_mapper.Map<TradeOffer>);
	}

	public async Task Add(TradeOffer offer,
		CancellationToken cancellationToken = default)
	{
		var mapped = _mapper.Map<EF.TradeOffer>(offer);
		await _writeCtx.AddAsync(mapped, cancellationToken);
	}

	public async Task DecreaseOfferAmount(Guid offerId, int amount)
	{
		var offer = await _writeCtx.GetById<EF.TradeOffer>(offerId)
			?? throw new KeyNotFoundException($"Offer: {offerId}");

		if (offer.Amount < amount)
			throw new ArgumentOutOfRangeException($"Offer: {offerId}");

		offer.Amount -= amount;
	}

	public void Cancel(Guid offerId)
	{
		var offer = _writeCtx.GetById<EF.TradeOffer>(offerId) ??
			throw new KeyNotFoundException($"Offer: {offerId}");

		_writeCtx.Remove(offer);
	}
}
