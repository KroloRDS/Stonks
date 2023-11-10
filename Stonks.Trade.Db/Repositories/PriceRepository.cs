using AutoMapper;
using Stonks.Trade.Domain.Models;
using Stonks.Trade.Domain.Repositories;
using CommonRepositories = Stonks.Common.Db.Repositories;

namespace Stonks.Trade.Db.Repositories;

public class PriceRepository : IPriceRepository
{
	private readonly IMapper _mapper;
	private readonly CommonRepositories.IPriceRepository _price;

	public PriceRepository(IMapper mapper,
		CommonRepositories.IPriceRepository price)
	{
		_mapper = mapper;
		_price = price;
	}

	public IEnumerable<AvgPrice> Prices(Guid? stockId = null,
		DateTime? fromDate = null, DateTime? toDate = null)
	{
		var prices = _price.Prices(stockId, fromDate, toDate);
		return prices.Select(_mapper.Map<AvgPrice>);
	}

	public async Task<decimal?> Current(Guid stockId)
	{
		var price = await _price.Current(stockId);
		return price?.Price;
	}
}
