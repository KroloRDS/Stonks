using Stonks.Common.Db;
using Stonks.Common.Db.EntityFrameworkModels;
using CommonRepositories = Stonks.Common.Db.Repositories;
using Stonks.Trade.Domain.Repositories;
using Stonks.Common.Utils.Models.Constants;

namespace Stonks.Trade.Db.Repositories;

public class ShareRepository : IShareRepository
{
	private readonly AppDbContext _writeCtx;
	private readonly ReadOnlyDbContext _readCtx;
	private readonly CommonRepositories.IShareRepository _share;

	public ShareRepository(AppDbContext writeCtx,
		ReadOnlyDbContext readCtx,
		CommonRepositories.IShareRepository share)
	{
		_writeCtx = writeCtx;
		_readCtx = readCtx;
		_share = share;
	}

	public async Task<int> GetOwnedAmount(Guid stockId, Guid userId,
		CancellationToken cancellationToken = default)
	{
		var ownership = await _readCtx.GetShares(
			userId, stockId, cancellationToken);

		return ownership?.Amount ?? 0;
	}

	public async Task<int> TotalAmountOfShares(Guid stockId,
		CancellationToken cancellationToken = default) =>
		await _share.TotalAmountOfShares(stockId, cancellationToken);

	public async Task GiveSharesToUser(Guid stockId, Guid userId,
		int amount, CancellationToken cancellationToken = default)
	{
		var ownership = await _writeCtx.GetShares(
			userId, stockId, cancellationToken);

		if (ownership is not null)
		{
			ownership.Amount += amount;
			return;
		}

		await _writeCtx.AddAsync(new Share
		{
			Amount = amount,
			OwnerId = userId,
			StockId = stockId
		}, cancellationToken);
	}

	public async Task TakeSharesFromUser(Guid stockId, Guid userId,
		int amount, CancellationToken cancellationToken = default)
	{
		await _writeCtx.EnsureExist<User>(userId, cancellationToken);
		var ownership = await _writeCtx.GetShares(userId,
			stockId, cancellationToken);

		if (ownership is null || ownership.Amount < amount)
			throw new NoStocksOnSellerException();

		ownership.Amount -= amount;
		if (ownership.Amount == 0) _writeCtx.Remove(ownership);
	}
}
