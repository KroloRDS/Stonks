using Stonks.Data;
using Stonks.Models;
using Stonks.Helpers;

namespace Stonks.Managers.Bankruptcy;

public class UpdatePriceManager : IUpdatePriceManager
{
	private readonly AppDbContext _ctx;

	public UpdatePriceManager(AppDbContext ctx)
	{
		_ctx = ctx;
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
		var currentPrice = _ctx.AvgPriceCurrent.SingleOrDefault(x => x.StockId == stockId);
		if (currentPrice is null)
		{
			AddFirstAvgPrice(stockId);
			return;
		}

		var transactions = GetNewTransactions(stockId, currentPrice!.Created);
		(var amount, var sharesTraded) = CalculateNewAverage(currentPrice, transactions);

		_ctx.Add(new AvgPrice
		{
			StockId = currentPrice.StockId,
			DateTime = currentPrice.Created,
			SharesTraded = currentPrice.SharesTraded,
			Amount = currentPrice.Amount,
			AmountNormalised = currentPrice.Amount
		});

		currentPrice.Created = DateTime.Now;
		currentPrice.SharesTraded = sharesTraded;
		currentPrice.Amount = amount;

		_ctx.SaveChanges();
	}

	private void AddFirstAvgPrice(Guid stockId)
	{
		var transactions = GetAllTransactions(stockId);
		var sharesTraded = 0UL;
		var priceSum = 0M;

		foreach (var transaction in transactions)
		{
			priceSum += transaction.Amount * transaction.Price;
			sharesTraded += (ulong)transaction.Amount;
		}
		var avgPrice = sharesTraded > 0 ?
			priceSum / sharesTraded :
			IUpdatePriceManager.DEFAULT_PRICE;

		_ctx.Add(new AvgPriceCurrent
		{
			StockId = stockId,
			Created = DateTime.Now,
			SharesTraded = sharesTraded,
			Amount = avgPrice
		});

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

	private List<Transaction> GetAllTransactions(Guid stockId)
	{
		return _ctx.Transaction
			.Where(x => x.StockId == stockId)
			.Select(x => new Transaction
			{
				Amount = x.Amount,
				Price = x.Price
			})
			.ToList();
	}

	private static (decimal, ulong) CalculateNewAverage(
		AvgPriceCurrent currentPrice, List<Transaction> transactions)
	{
		var sharesTraded = currentPrice.SharesTraded;
		var priceSum = currentPrice.Amount * sharesTraded;
		foreach (var transaction in transactions)
		{
			priceSum += transaction.Amount * transaction.Price;
			sharesTraded += (ulong)transaction.Amount;
		}
		var avgPrice = sharesTraded > 0 ? priceSum / sharesTraded : 0M;

		return (avgPrice, sharesTraded);
	}
}
