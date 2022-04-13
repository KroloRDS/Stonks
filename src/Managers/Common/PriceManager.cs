using Stonks.Data;
using Stonks.Models;
using Stonks.Helpers;

namespace Stonks.Managers.Common;

public class PriceManager : IPriceManager
{
	private readonly AppDbContext _ctx;
	public const decimal DEFAULT_PRICE = 1M;

	public PriceManager(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public AvgPrice GetCurrentPrice(Guid? stockId)
	{
		var stock = _ctx.GetById<Stock>(stockId);
		if (stock.Bankrupt)
			throw new BankruptStockException();

		return GetCurrentPriceForValidStock(stock.Id);
	}

	public List<AvgPrice> GetHistoricalPrices(
		Guid? stockId, DateTime? fromDate, DateTime? toDate = null)
	{
		if (stockId is null)
			throw new ArgumentNullException(nameof(stockId));
		if (fromDate is null)
			throw new ArgumentNullException(nameof(fromDate));
		else if (toDate < fromDate)
			throw new ArgumentOutOfRangeException(nameof(toDate));

		if (toDate is null)
		{
			return _ctx.AvgPrice
				.Where(x => x.StockId == stockId && x.DateTime >= fromDate)
				.ToList();
		}

		return _ctx.AvgPrice
			.Where(x => x.StockId == stockId && x.DateTime >= fromDate && x.DateTime <= toDate)
			.ToList();
	}

	private AvgPrice GetCurrentPriceForValidStock(Guid stockId)
	{
		if (!_ctx.AvgPrice.Any(x => x.StockId == stockId))
		{
			_ctx.Add(new AvgPrice
			{
				StockId = stockId,
				DateTime = DateTime.MinValue,
				IsCurrent = true,
				TotalAmountTraded = 0UL,
				Amount = DEFAULT_PRICE,
				AmountNormalised = DEFAULT_PRICE
			});
			_ctx.SaveChanges();
		}
		return _ctx.AvgPrice.Single(x => x.IsCurrent && x.StockId == stockId);
	}

	public void UpdateAveragePrices()
	{
		var ids = _ctx.Stock
			.Where(x => !x.Bankrupt)
			.Select(x => x.Id);

		foreach (var id in ids)
		{
			UpdateAveragePriceForValidStock(id);
		}
	}

	public void UpdateAveragePriceForOneStock(Guid? stockId)
	{
		var stock = _ctx.GetById<Stock>(stockId);
		if (stock.Bankrupt)
			throw new BankruptStockException();

		UpdateAveragePriceForValidStock(stock.Id);
	}

	private void UpdateAveragePriceForValidStock(Guid stockId)
	{
		var previousTransactions = GetCurrentPriceForValidStock(stockId);
		var transactions = GetNewTransactions(stockId, previousTransactions.DateTime);

		var amount = previousTransactions.TotalAmountTraded;
		var priceSum = previousTransactions.Amount * amount;
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
		_ctx.Add(new AvgPrice
		{
			StockId = stockId,
			DateTime = DateTime.Now,
			IsCurrent = true,
			TotalAmountTraded = amount,
			Amount = price,
			AmountNormalised = price
		});
	}
}
