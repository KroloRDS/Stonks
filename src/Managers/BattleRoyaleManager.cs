using Stonks.Data;
using Stonks.Helpers;

namespace Stonks.Managers;

public class BattleRoyaleManager : IBattleRoyaleManager
{
	private readonly IHistoricalPriceManager _historicalPriceManager;
	private readonly IStockOwnershipManager _ownershipManager;
	private readonly IStockManager _stockManager;
	private readonly IConfigurationManager _config;
	private readonly AppDbContext _ctx;

	public BattleRoyaleManager(AppDbContext ctx, IHistoricalPriceManager historicalPriceManager,
		IStockOwnershipManager ownershipManager, IStockManager stockManager,
		IConfigurationManager config)
	{
		_ctx = ctx;
		_historicalPriceManager = historicalPriceManager;
		_ownershipManager = ownershipManager;
		_stockManager = stockManager;
		_config = config;
	}

	private class StockIndicator
	{
		public Guid StockId { get; set; }
		public decimal MarketCap { get; set; }
		public double MarketCapNormalised { get; set; }
		public int StocksAmount { get; set; }
		public double StocksAmountNormalised { get; set; }
		public double Volatility { get; set; }
		public double Fun { get; set; }
	}

	public void BattleRoyaleRound()
	{
		Eliminate();
		var amount = _config.NewStocksAfterRound();
		_stockManager.EmitNewStocks(amount);
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
			StocksAmount = _stockManager.GetPublicStocksAmount(stockId),
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
		var lastBankruptDate = _stockManager.GetLastBankruptDate() ?? new DateTime(2021, 1, 1);
		return _historicalPriceManager.GetHistoricalPrices(stockId, lastBankruptDate)
			.Select(x => x.AveragePrice).StandardDev();
	}

	private static void NormaliseIndicators(ref List<StockIndicator> indicators)
	{
		var minMarketCap = indicators.Min(x => x.MarketCap);
		var marketCapMaxDiff = indicators.Max(x => x.MarketCap) - minMarketCap;

		var minVolatility = indicators.Min(x => x.Volatility);
		var volatilityMaxDiff = indicators.Max(x => x.Volatility) - minVolatility;

		var mostStocksAvailable = indicators.Max(x => x.StocksAmount);

		foreach (var indicator in indicators)
		{
			indicator.MarketCapNormalised = (double)((indicator.MarketCap - minMarketCap) / marketCapMaxDiff);
			indicator.StocksAmountNormalised = (mostStocksAvailable - indicator.StocksAmount) / mostStocksAvailable;
			indicator.Volatility = (indicator.Volatility - minVolatility) / volatilityMaxDiff;
		}
	}

	private double GetScore(StockIndicator indicator)
	{
		return _config.MarketCapWeight() * indicator.MarketCapNormalised +
			_config.StockAmountWeight() * indicator.StocksAmountNormalised +
			_config.VolatilityWeight() * indicator.Volatility +
			_config.FunWeight() * indicator.Fun;
	}
}
