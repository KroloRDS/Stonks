using Stonks.Common.Utils;
using Stonks.Common.Utils.Response;
using Stonks.Trade.Application.DTOs;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Application.Requests;

public class GetStocksHandler
{
	private readonly IOfferRepository _offer;
	private readonly IShareRepository _share;
	private readonly IStockRepository _stock;
	private readonly IPriceRepository _price;
	private readonly IStonksLogger<GetUserOffersHandler> _logger;

	public GetStocksHandler(
		IOfferRepository offer,
		IShareRepository share,
		IStockRepository stock,
		IPriceRepository price,
		IStonksLogger<GetUserOffersHandler> logger)
	{
		_offer = offer;
		_share = share;
		_stock = stock;
		_price = price;
		_logger = logger;
	}

	public async Task<Response<IEnumerable<StockDTO>>> Handle(
		CancellationToken cancellationToken)
	{
		try
		{
			var stocks = await GetStocks(cancellationToken);
			return Response<IEnumerable<StockDTO>>.Ok(stocks);
		}
		catch (Exception ex)
		{
			_logger.Log(ex);
			return Response<IEnumerable<StockDTO>>.Error(ex);
		}
	}

	public async Task<IEnumerable<StockDTO>> GetStocks(
		CancellationToken cancellationToken)
	{
		var tickers = await _stock.GetTickers(cancellationToken);
		var tasks = tickers.Select(x =>
			GetStockDTO(x.Key, x.Value, cancellationToken));

		var stocks = await Task.WhenAll(tasks);
		return stocks;
	}

	private async Task<StockDTO> GetStockDTO(Guid stockId,
		string ticker, CancellationToken cancellationToken)
	{
		var volatility = GetVolatility(stockId, cancellationToken);
		var price = await _price.Current(stockId);
		var marketCap = GetMarketCap(stockId, price, cancellationToken);
		var publicallyOfferdShares = _offer.PublicallyOfferdAmount(
			stockId, cancellationToken);

		return new StockDTO
		{
			Ticker = ticker,
			AvgPrice = price,
			MarketCap = await marketCap,
			Volatility = await volatility,
			PublicallyOfferdShares = await publicallyOfferdShares
		};
	}

	private async Task<decimal> GetMarketCap(Guid stockId,
		decimal price, CancellationToken cancellationToken)
	{
		var shares = await _share.TotalAmountOfShares(stockId, cancellationToken);
		return shares * price;
	}

	private async Task<double> GetVolatility(
		Guid stockId, CancellationToken cancellationToken)
	{
		var lastBankruptDate = await _stock.LastBankruptDate(cancellationToken);
		var prices = _price.Prices(stockId, lastBankruptDate);
		return prices.Select(x => x.Price).StandardDev();
	}
}
