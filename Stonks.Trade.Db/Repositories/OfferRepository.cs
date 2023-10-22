using AutoMapper;
using Stonks.Trade.Domain.Models;
using Stonks.Trade.Domain.Repositories;
using Stonks.Common.Db;
using EF = Stonks.Common.Db.EntityFrameworkModels;
using CommonRepositories = Stonks.Common.Db.Repositories;

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

	public async Task<TradeOffer?> Get(Guid offerId)
	{
		var offer = await _readCtx.GetById<EF.TradeOffer>(offerId);
		return offer is null ? null : _mapper.Map<TradeOffer>(offer);
	}

	public IEnumerable<TradeOffer> GetUserBuyOffers(Guid userId)
	{
		var offers = _readCtx.TradeOffer.Where(x =>
		   x.Type == EF.OfferType.Buy &&
		   x.WriterId == userId)
			.ToList();

		return offers.Select(_mapper.Map<TradeOffer>);
	}

	public IEnumerable<TradeOffer> GetUserSellOffers(Guid userId)
	{
		var offers = _readCtx.TradeOffer.Where(x =>
		   x.Type == EF.OfferType.Sell &&
		   x.WriterId == userId)
			.ToList();

		return offers.Select(_mapper.Map<TradeOffer>);
	}

	public IEnumerable<TradeOffer> FindBuyOffers(
		Guid stockId, decimal price)
	{
		var offers = _readCtx.TradeOffer.Where(x =>
		   x.Type == EF.OfferType.Buy &&
		   x.StockId == stockId &&
		   x.Price >= price)
			.OrderByDescending(x => x.Price)
			.ToList();

		return offers.Select(_mapper.Map<TradeOffer>);
	}

	public IEnumerable<TradeOffer> FindSellOffers(
		Guid stockId, decimal price)
	{
		var offers = _readCtx.TradeOffer.Where(x =>
			x.Type != EF.OfferType.Buy &&
			x.StockId == stockId &&
			x.Price <= price)
			.OrderBy(x => x.Price)
			.ToList();

		return offers.Select(_mapper.Map<TradeOffer>);
	}

	public async Task Add(TradeOffer offer,
		CancellationToken cancellationToken = default)
	{
		var mapped = _mapper.Map<EF.TradeOffer>(offer);
		await _writeCtx.AddAsync(mapped, cancellationToken);
	}

	public bool Cancel(Guid offerId)
	{
		var offer = _writeCtx.GetById<EF.TradeOffer>(offerId);
		if (offer is null) return false;
		_writeCtx.Remove(offer);
		return true;
	}
}
