using Stonks.Data;
using Stonks.Models;

namespace Stonks.Managers;

public class HistoricalPriceManager : IHistoricalPriceManager
{
	private readonly AppDbContext _ctx;

	public HistoricalPriceManager(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public HistoricalPrice GetCurrentPrice(Guid? stockId)
	{
		var validStockId = _ctx.EnsureExist<Stock>(stockId);
		return GetCurrentPriceForValidStock(validStockId);
	}

	private HistoricalPrice GetCurrentPriceForValidStock(Guid stockId)
	{
		if (!_ctx.HistoricalPrice.Any(x => x.StockId == stockId))
		{
			_ctx.Add(new HistoricalPrice
			{
				StockId = stockId,
				DateTime = DateTime.MinValue,
				IsCurrent = true,
				TotalAmountTraded = 0UL,
				AveragePrice = 0M,
				PriceNormalised = 0M
			});
			_ctx.SaveChanges();
		}
		return _ctx.HistoricalPrice.Single(x => x.IsCurrent && x.StockId == stockId);
	}

	public void UpdateAveragePrices()
	{
		var ids = _ctx.Stock.Select(x => x.Id);
		foreach (var id in ids)
		{
			UpdateAveragePriceForOneStock(id);
		}
	}

	public void UpdateAveragePriceForOneStock(Guid stockId)
	{
		var previousTransactions = GetCurrentPriceForValidStock(stockId);
		var transactions = GetNewTransactions(stockId, previousTransactions.DateTime);

		var amount = previousTransactions.TotalAmountTraded;
		var priceSum = previousTransactions.AveragePrice * amount;
		foreach (var transaction in transactions)
		{
			priceSum += transaction.Amount * transaction.Price;
			amount += (ulong)transaction.Amount;
		}
		var avgPrice = amount > 0 ? priceSum / amount : 0M;

		previousTransactions.IsCurrent = false;
		AddNewHistoricalPrice(stockId, amount, avgPrice);
		_ctx.SaveChanges();
	}

	private List<Transaction> GetNewTransactions(Guid stockId, DateTime dateTime)
	{
		return _ctx.Transaction
			.Where(x => x.StockId == stockId &&
			x.Timestamp >= dateTime)
			.Select(x => new Transaction
			{
				Amount = x.Amount,
				Price = x.Price
			})
			.ToList();
	}
	
	private void AddNewHistoricalPrice(Guid stockId, ulong amount, decimal price)
	{
		_ctx.Add(new HistoricalPrice
		{
			StockId = stockId,
			DateTime = DateTime.Now,
			IsCurrent = true,
			TotalAmountTraded = amount,
			AveragePrice = price,
			PriceNormalised = price
		});
	}
}
