using Microsoft.EntityFrameworkCore;
using Stonks.Common.Db.EntityFrameworkModels;

namespace Stonks.Common.Db.Repositories;

public interface IPriceRepository
{
	Task<AvgPrice?> Current(Guid stockId,
		CancellationToken cancellationToken = default);

	IEnumerable<AvgPrice> Prices(Guid? stockId = null,
		DateTime? fromDate = null, DateTime? toDate = null);
}

public class PriceRepository : IPriceRepository
{
	private readonly ReadOnlyDbContext _ctx;

	public PriceRepository(ReadOnlyDbContext ctx)
	{
		_ctx = ctx;
	}

	public async Task<AvgPrice?> Current(Guid stockId,
		CancellationToken cancellationToken = default)
	{
		var price = await _ctx.AvgPrice
			.Where(x => x.StockId == stockId)
			.OrderByDescending(x => x.DateTime)
			.FirstOrDefaultAsync(cancellationToken);
		return price;
	}

	public IEnumerable<AvgPrice> Prices(Guid? stockId = null,
		DateTime? fromDate = null, DateTime? toDate = null)
	{
		if (fromDate is not null &&
			toDate is not null &&
			toDate <= fromDate)
			throw new ArgumentOutOfRangeException(nameof(toDate));

		var query = (AvgPrice x) => !stockId.HasValue ||
			x.StockId == stockId;

		var queryFrom = fromDate is null ? query :
			(AvgPrice x) => query(x) && x.DateTime >= fromDate;

		var queryTo = toDate is null ? queryFrom :
			(AvgPrice x) => queryFrom(x) && x.DateTime <= toDate;

		var prices = _ctx.AvgPrice
			.Where(queryTo)
			.OrderBy(x => x.DateTime)
			.ToList();

		return prices;
	}
}
