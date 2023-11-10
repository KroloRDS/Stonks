using Stonks.Administration.Domain.Repositories;
using Stonks.Common.Db;
using CommonRepositories = Stonks.Common.Db.Repositories;

namespace Stonks.Administration.Db.Repositories;

public class ShareRepository : IShareRepository
{
	private readonly AppDbContext _ctx;
	private readonly CommonRepositories.IShareRepository _share;

	public ShareRepository(AppDbContext ctx,
		CommonRepositories.IShareRepository share)
	{
		_ctx = ctx;
		_share = share;
	}

	public async Task<int> TotalAmountOfShares(Guid stockId,
		CancellationToken cancellationToken = default) =>
		await _share.TotalAmountOfShares(stockId, cancellationToken);

	public void RemoveShares(Guid stockId) =>
		_ctx.Share.RemoveRange(
			_ctx.Share.Where(x => x.StockId == stockId));
}
