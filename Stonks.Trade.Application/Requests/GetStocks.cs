using MediatR;
using Stonks.Common.Utils.ExtensionMethods;
using Stonks.Common.Utils.Models;
using Stonks.Common.Utils.Models.Constants;
using Stonks.Common.Utils.Services;
using Stonks.Trade.Application.DTOs;
using Stonks.Trade.Domain.Models;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Application.Requests;

public record GetStocks : IRequest<Response<IEnumerable<StockDTO>>>;

public class GetStocksHandler : 
	IRequestHandler<GetStocks, Response<IEnumerable<StockDTO>>>
{
	private readonly IOfferRepository _offer;
	private readonly IShareRepository _share;
	private readonly IStockRepository _stock;
	private readonly IPriceRepository _price;
	private readonly IStonksLogger _logger;

	public GetStocksHandler(
		IOfferRepository offer,
		IShareRepository share,
		IStockRepository stock,
		IPriceRepository price,
		ILogProvider logProvider)
	{
		_offer = offer;
		_share = share;
		_stock = stock;
		_price = price;
		_logger = new StonksLogger(logProvider, GetType().Name);
	}

	public async Task<Response<IEnumerable<StockDTO>>> Handle(
		GetStocks request, CancellationToken cancellationToken = default)
	{
		try
		{
			var stocks = await GetStocks(cancellationToken);
			return Response.Ok(stocks);
		}
		catch (Exception ex)
		{
			_logger.Log(ex);
			return Response.Error(ex);
		}
	}

	public async Task<IEnumerable<StockDTO>> GetStocks(
		CancellationToken cancellationToken = default)
	{
		var tickers = _stock.GetStockNames();
		var tasks = tickers.Select(x =>
			GetStockDTO(x.Key, x.Value, cancellationToken));

		var stocks = await Task.WhenAll(tasks);
		return stocks;
	}

	private async Task<StockDTO> GetStockDTO(Guid stockId, Stock stock,
		CancellationToken cancellationToken = default)
	{
		var volatility = GetVolatility(stockId, cancellationToken);
		var price = await _price.Current(stockId) ??
			Constants.STOCK_DEFAULT_PRICE;
		var marketCap = GetMarketCap(stockId, price, cancellationToken);
		var publicallyOfferdShares = _offer.PublicallyOfferdAmount(
			stockId, cancellationToken);

		return new StockDTO
		{
			Name = stock.Name,
			Ticker = stock.Ticker,
			AvgPrice = price,
			MarketCap = await marketCap,
			Volatility = await volatility,
			PublicallyOfferdShares = await publicallyOfferdShares
		};
	}

	private async Task<decimal> GetMarketCap(Guid stockId, decimal price,
		CancellationToken cancellationToken = default)
	{
		var shares = await _share.TotalAmountOfShares(stockId, cancellationToken);
		return shares * price;
	}

	private async Task<double> GetVolatility(Guid stockId,
		CancellationToken cancellationToken = default)
	{
		var lastBankruptDate = await _stock.LastBankruptDate(cancellationToken);
		var prices = _price.Prices(stockId, lastBankruptDate);
		return prices.Select(x => x.Price).StandardDev();
	}
}
