using Stonks.Data;
using Stonks.DTOs;
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

	public void BattleRoyaleRound()
	{
		var toEliminate = GetWeakestStockId();
		var amount = _config.NewStocksAfterRound();
		_stockManager.Bankrupt(toEliminate);
		_stockManager.EmitNewStocks(amount);
	}

	public Guid GetWeakestStockId()
	{
		var stocks = _ctx.Stock.Where(x => !x.Bankrupt);
		if (!stocks.Any())
			throw new Exception("No stocks to bankrupt");

		return stocks.Select(x => GetStockIndicator(x.Id))
			.ToList()
			.Normalise()
			.OrderBy(x => GetScore(x))
			.First().StockId;
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

	private double GetScore(StockIndicatorNormalised indicator)
	{
		return _config.MarketCapWeight() * indicator.MarketCap +
			_config.StockAmountWeight() * indicator.StocksAmount +
			_config.VolatilityWeight() * indicator.Volatility +
			_config.FunWeight() * indicator.Fun;
	}
}
