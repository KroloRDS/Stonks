using Microsoft.EntityFrameworkCore;

namespace Stonks.Common.Db.Repositories;

public interface IStockRepository
{
	Task<DateTime?> LastBankruptDate(
		CancellationToken cancellationToken = default);
}

public class StockRepository : IStockRepository
{
	private readonly ReadOnlyDbContext _ctx;

	public StockRepository(ReadOnlyDbContext ctx)
	{
		_ctx = ctx;
	}

	public async Task<DateTime?> LastBankruptDate(
		CancellationToken cancellationToken = default)
	{
		var stocks = _ctx.Stock.Where(x => x.BankruptDate.HasValue)
			.Select(x => x.BankruptDate!.Value);

		var hasBankrupted = await stocks.AnyAsync(cancellationToken);

		return hasBankrupted ?
			await stocks.MaxAsync(cancellationToken) : null;
	}
}
