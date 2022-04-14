using Stonks.Data;
using Stonks.DTOs;
using Stonks.Helpers;
using Stonks.Managers.Common;

namespace Stonks.Managers.Bankruptcy;

public class BattleRoyaleManager : IBattleRoyaleManager
{
	private readonly IGetPriceManager _priceManager;
	private readonly IBankruptSharesManager _shareManager;
	private readonly IBankruptStockManager _stockManager;
	private readonly IConfigurationManager _config;
	private readonly AppDbContext _ctx;

	public BattleRoyaleManager(AppDbContext ctx, IGetPriceManager historicalPriceManager,
		IBankruptSharesManager ownershipManager, IBankruptStockManager stockManager,
		IConfigurationManager config)
	{
		_ctx = ctx;
		_priceManager = historicalPriceManager;
		_shareManager = ownershipManager;
		_stockManager = stockManager;
		_config = config;
	}

	public void BattleRoyaleRound()
	{
		var toEliminate = GetWeakestStockId();
		var amount = _config.NewStocksAfterRound();
		_stockManager.Bankrupt(toEliminate);
		_stockManager.EmitNewStocks(amount);
		_ctx.SaveChanges();
	}

	public Guid GetWeakestStockId()
	{
		var stocks = _ctx.Stock.Where(x => !x.Bankrupt);
		if (!stocks.Any())
			throw new NoStocksToBankruptException();

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
		var price = _priceManager.GetCurrentPrice(stockId).Amount;
		return _shareManager.GetAllSharesAmount(stockId) * price;
	}

	private double GetVolatility(Guid stockId)
	{
		var lastBankruptDate = _stockManager.GetLastBankruptDate() ?? new DateTime(2021, 1, 1);
		return _priceManager.GetHistoricalPrices(stockId, lastBankruptDate)
			.Select(x => x.Amount).StandardDev();
	}

	private double GetScore(StockIndicatorNormalised indicator)
	{
		return _config.MarketCapWeight() * indicator.MarketCap +
			_config.StockAmountWeight() * indicator.StocksAmount +
			_config.VolatilityWeight() * indicator.Volatility +
			_config.FunWeight() * indicator.Fun;
	}
}
