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
		stock.PublicallyOfferredAmount = 0;

		_tradeManager.RemoveAllOffersForStock(stockId);
		_ownershipManager.RemoveAllOwnershipForStock(stockId);
	}

	public DateTime? GetLastBankruptDate()
	{
		var bankrupted = _ctx.Stock.Where(x => x.BankruptDate.HasValue)
			.Select(x => x.BankruptDate!.Value);

		return bankrupted.Any() ? bankrupted.Max() : null;
	}

	public int GetPublicStocksAmount(Guid? stockId)
	{
		var stock = _ctx.GetById<Stock>(stockId);
		return stock.PublicallyOfferredAmount;
	}

	public void EmitNewStocks(int amount)
	{
		var stocks = _ctx.Stock.Where(x => !x.Bankrupt);
		foreach (var stock in stocks)
		{
			if (stock.PublicallyOfferredAmount < amount)
				stock.PublicallyOfferredAmount = amount;
		}

		_tradeManager.AddPublicOffers(amount);
	}
}