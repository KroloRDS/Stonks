using Microsoft.EntityFrameworkCore;

namespace Stonks.Common.Db.Repositories;

public interface IShareRepository
{
	Task<int> TotalAmountOfShares(Guid stockId,
		CancellationToken cancellationToken = default);
}

public class ShareRepository : IShareRepository
{
	private readonly ReadOnlyDbContext _ctx;

	public ShareRepository(ReadOnlyDbContext ctx)
	{
		_ctx = ctx;
	}

	public async Task<int> TotalAmountOfShares(Guid stockId,
		CancellationToken cancellationToken = default)
	{
		var sum = await _ctx.Share
			.Where(x => x.StockId == stockId)
			.SumAsync(x => x.Amount, cancellationToken);
		return sum;
	}
}
