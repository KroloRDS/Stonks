using Stonks.Data;
using Stonks.Models;

namespace Stonks.Managers;
public class StockManager : IStockManager
{
	private readonly IStockOwnershipManager _ownershipManager;
	private readonly ITradeManager _tradeManager;
	private readonly AppDbContext _ctx;

	public StockManager(AppDbContext ctx, IStockOwnershipManager ownershipManager,
		ITradeManager tradeManager)
	{
		_ctx = ctx;
		_tradeManager = tradeManager;
		_ownershipManager = ownershipManager;
	}

	public void Bankrupt(Guid? stockId)
	{
		var stock = _ctx.GetById<Stock>(stockId);
		stock.Bankrupt = true;
		stock.BankruptDate = DateTime.Now;
		_ctx.SaveChanges();

		_tradeManager.RemoveAllOffersForStock(stockId);
		_ownershipManager.RemoveAllOwnershipForStock(stockId);
	}

	public DateTime GetLastBankruptDate()
	{
		return _ctx.Stock.Where(x => x.BankruptDate.HasValue)
			.Select(x => x.BankruptDate!.Value).Max();
	}
}