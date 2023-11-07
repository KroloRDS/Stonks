using AutoMapper;
using Stonks.Administration.Domain.Models;
using Stonks.Administration.Domain.Repositories;
using Stonks.Common.Db;
using CommonRepositories = Stonks.Common.Db.Repositories;

namespace Stonks.Administration.Db.Repositories;

public class PriceRepository : IPriceRepository
{
	private readonly AppDbContext _ctx;
	private readonly IMapper _mapper;
	private readonly CommonRepositories.IPriceRepository _price;

	public PriceRepository(AppDbContext ctx, IMapper mapper,
		CommonRepositories.IPriceRepository price)
	{
		_ctx = ctx;
		_price = price;
		_mapper = mapper;
	}

	public async Task Add(AvgPrice price,
		CancellationToken cancellationToken = default)
	{
		await _ctx.AddAsync(price, cancellationToken);
	}

	public IEnumerable<AvgPrice> Prices(Guid? stockId = null,
		DateTime? fromDate = null, DateTime? toDate = null)
	{
		var prices = _price.Prices(stockId, fromDate, toDate);
		return prices.Select(_mapper.Map<AvgPrice>);
	}

	public async Task<AvgPrice?> Current(Guid stockId)
	{
		var price = await _price.Current(stockId);
		return _mapper.Map<AvgPrice?>(price);
	}
}
