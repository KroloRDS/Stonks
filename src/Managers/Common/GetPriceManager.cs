using Stonks.Data;
using Stonks.Models;
using Stonks.Helpers;

namespace Stonks.Managers.Common;

public class GetPriceManager : IGetPriceManager
{
	private readonly AppDbContext _ctx;

	public GetPriceManager(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public AvgPriceCurrent GetCurrentPrice(Guid? stockId)
	{
		var stock = _ctx.GetById<Stock>(stockId);
		if (stock.Bankrupt)
			throw new BankruptStockException();

		var price = _ctx.AvgPriceCurrent.SingleOrDefault(x => x.StockId == stockId);
		if (price is null)
			throw new NoCurrentPriceException();

		return price;
	}

	public IEnumerable<AvgPrice> GetHistoricalPrices(
		Guid? stockId, DateTime? fromDate, DateTime? toDate = null)
	{
		if (stockId is null)
			throw new ArgumentNullException(nameof(stockId));
		if (fromDate is null)
			throw new ArgumentNullException(nameof(fromDate));
		else if (toDate < fromDate)
			throw new ArgumentOutOfRangeException(nameof(toDate));

		if (toDate is null)
		{
			return _ctx.AvgPrice
				.Where(x => x.StockId == stockId && x.DateTime >= fromDate)
				.OrderBy(x => x.DateTime)
				.ToList();
		}

		return _ctx.AvgPrice
			.Where(x => x.StockId == stockId && x.DateTime >= fromDate && x.DateTime <= toDate)
			.OrderBy(x => x.DateTime)
			.ToList();
	}
}
