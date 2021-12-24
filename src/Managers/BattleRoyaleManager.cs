using Stonks.Data;
using Stonks.Models;
using Stonks.Helpers;

namespace Stonks.Managers;

public class BattleRoyaleManager : IBattleRoyaleManager
{
	private readonly IHistoricalPriceManager _historicalPriceManager;
	private readonly IStockOwnershipManager _ownershipManager;
	private readonly IStockManager _stockManager;
	private readonly AppDbContext _ctx;

	public BattleRoyaleManager(AppDbContext ctx, IHistoricalPriceManager historicalPriceManager,
		IStockOwnershipManager ownershipManager, IStockManager stockManager)
	{
		_ctx = ctx;
		_historicalPriceManager = historicalPriceManager;
		_ownershipManager = ownershipManager;
		_stockManager = stockManager;
	}

	private class StockIndicator
	{
		public Guid StockId { get; set; }
		public decimal MarketCap { get; set; }
		public double MarketCapNormalised { get; set; }
		public double Volatility { get; set; }
		public double Fun { get; set; }
	}

	public void BattleRoyaleRound()
	{
		Eliminate();
		//TODO: Emit new stocks
	}

	private void Eliminate()
	{
		var stocks = _ctx.Stock.Where(x => !x.Bankrupt).
			Select(x => GetStockIndicator(x.Id)).ToList();

		NormaliseIndicators(ref stocks);
		var stockToEliminate = stocks.OrderBy(x => GetScore(x))
			.First().StockId;

		_stockManager.Bankrupt(stockToEliminate);
	}

	private StockIndicator GetStockIndicator(Guid stockId)
	{
		return new StockIndicator
		{
			StockId = stockId,
			MarketCap = GetMarketCap(stockId),
			Volatility = GetVolatility(stockId),
			Fun = new Random().NextDouble()
		};
	}

	private decimal GetMarketCap(Guid stockId)
	{
		var price = _historicalPriceManager.GetCurrentPrice(stockId).AveragePrice;
		return _ownershipManager.GetAllOwnedStocksAmount(stockId) * price;
	}

	private double GetVolatility(Guid stockId)
	{
		var lastBankruptDate = _stockManager.GetLastBankruptDate();
		return _historicalPriceManager.GetHistoricalPrices(stockId, lastBankruptDate)
			.Select(x => x.AveragePrice).StandardDev();
	}

	private static void NormaliseIndicators(ref List<StockIndicator> indicators)
	{
		var maxMarketCap = (double)indicators.Max(x => x.MarketCap);
		var minMarketCap = (double)indicators.Min(x => x.MarketCap);

		var maxVolatility = indicators.Max(x => x.Volatility);
		var minVolatility = indicators.Min(x => x.Volatility);

		foreach (var indicator in indicators)
		{
			indicator.MarketCapNormalised = ((double)indicator.MarketCap - minMarketCap) / maxMarketCap;
			indicator.Volatility = (indicator.Volatility - minVolatility) / maxVolatility;
		}
	}

	private double GetScore(StockIndicator indicator)
	{
		//TODO: Add weights
		return indicator.MarketCapNormalised + indicator.Volatility + indicator.Fun;
	}
}
