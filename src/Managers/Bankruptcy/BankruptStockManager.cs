using Stonks.Data;
using Stonks.Models;

namespace Stonks.Managers.Bankruptcy;
public class BankruptStockManager : IBankruptStockManager
{
	private readonly IBankruptSharesManager _shareManager;
	private readonly IBankruptTradeOfferManager _offerManager;
	private readonly AppDbContext _ctx;

	public BankruptStockManager(AppDbContext ctx, IBankruptSharesManager ownershipManager,
		IBankruptTradeOfferManager tradeManager)
	{
		_ctx = ctx;
		_offerManager = tradeManager;
		_shareManager = ownershipManager;
	}

	public void Bankrupt(Guid? stockId)
	{
		var stock = _ctx.GetById<Stock>(stockId);
		stock.Bankrupt = true;
		stock.BankruptDate = DateTime.Now;
		stock.PublicallyOfferredAmount = 0;

		_offerManager.RemoveAllOffersForStock(stockId);
		_shareManager.RemoveAllShares(stockId);
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
		if (amount <= 0)
			throw new ArgumentOutOfRangeException(nameof(amount));

		var stocks = _ctx.Stock.Where(x => !x.Bankrupt);
		foreach (var stock in stocks)
		{
			if (stock.PublicallyOfferredAmount < amount)
				stock.PublicallyOfferredAmount = amount;
		}

		_offerManager.AddPublicOffers(amount);
	}
}