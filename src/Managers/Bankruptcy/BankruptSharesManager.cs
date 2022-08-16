using Stonks.Data;
using Stonks.Models;

namespace Stonks.Managers.Bankruptcy;

public class BankruptSharesManager : IBankruptSharesManager
{
	private readonly AppDbContext _ctx;

	//TODO: Ensure availability & safty for multiple threads
	public BankruptSharesManager(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public void RemoveAllShares(Guid? stockId)
	{
		_ctx.EnsureExist<Stock>(stockId);
		_ctx.RemoveRange(_ctx.Share.Where(x => x.StockId == stockId));
	}

	public int GetTotalAmountOfShares(Guid? stockId)
	{
		if (stockId is null)
		{
			throw new ArgumentNullException(nameof(stockId));
		}

		var amounts = _ctx.Share.Where(x => x.StockId == stockId)
			.Select(x => x.Amount);

		return amounts.Any() ? amounts.Sum() : 0;
	}
}
