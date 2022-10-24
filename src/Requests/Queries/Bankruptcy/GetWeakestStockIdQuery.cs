using MediatR;
using Microsoft.EntityFrameworkCore;

using Stonks.Data;
using Stonks.DTOs;
using Stonks.Providers;
using Stonks.CustomExceptions;
using Stonks.ExtensionMethods;
using Stonks.Responses.Bankruptcy;
using Stonks.Requests.Queries.Common;

namespace Stonks.Requests.Queries.Bankruptcy;

public record GetWeakestStockIdQuery : IRequest<GetWeakestStockIdResponse>;

public class GetWeakestStockIdQueryHandler :
	IRequestHandler<GetWeakestStockIdQuery, GetWeakestStockIdResponse>
{
	private readonly AppDbContext _ctx;
	private readonly IMediator _mediator;
	private readonly IStonksConfiguration _config;

	public GetWeakestStockIdQueryHandler(AppDbContext ctx,
		IMediator mediator, IStonksConfiguration config)
	{
		_ctx = ctx;
		_mediator = mediator;
		_config = config;
	}

	public async Task<GetWeakestStockIdResponse> Handle(
		GetWeakestStockIdQuery request, CancellationToken cancellationToken)
	{
		var stocks = await _ctx.Stock.Where(x => !x.Bankrupt)
			.Select(x => x.Id)
			.ToListAsync(cancellationToken);

		if (!stocks.Any())
			throw new NoStocksToBankruptException();

		var indicators = await Task.WhenAll(stocks
			.Select(x => GetStockIndicator(x, cancellationToken)));

		var worstStock = indicators.Normalise()
			.OrderBy(x => GetScore(x))
			.First();

		return new GetWeakestStockIdResponse(worstStock.StockId);
	}

	private async Task<StockIndicator> GetStockIndicator(
		Guid stockId, CancellationToken cancellationToken)
	{
		var publicStocks = _mediator.Send(
			new GetPublicStocksAmountQuery(stockId), cancellationToken);
		var marketCap = GetMarketCap(stockId, cancellationToken);
		var volatility = GetVolatility(stockId, cancellationToken);

		return new StockIndicator
		{
			StockId = stockId,
			MarketCap = await marketCap,
			StocksAmount = (await publicStocks).Amount,
			Volatility = await volatility,
			Fun = new Random().NextDouble()
		};
	}

	private async Task<decimal> GetMarketCap(
		Guid stockId, CancellationToken cancellationToken)
	{
		var price = _mediator.Send(
			new GetCurrentPriceQuery(stockId), cancellationToken);
		var shares = _mediator.Send(
			new GetTotalAmountOfSharesQuery(stockId), cancellationToken);
		return (await shares).Amount * (await price).Price;
	}

	private async Task<double> GetVolatility(
		Guid stockId, CancellationToken cancellationToken)
	{
		var lastBankruptDate = await _mediator.Send(
			new GetLastBankruptDateQuery(), cancellationToken);

		var result = await _mediator.Send(new GetHistoricalPricesQuery
		{
			FromDate = lastBankruptDate.DateTime,
			StockId = stockId
		},
		cancellationToken);

		return result.Prices.Select(x => x.Price).StandardDev();
	}

	private double GetScore(StockIndicatorNormalised indicator)
	{
		return _config.MarketCapWeight() * indicator.MarketCap +
			_config.StockAmountWeight() * indicator.StocksAmount +
			_config.VolatilityWeight() * indicator.Volatility +
			_config.FunWeight() * indicator.Fun;
	}
}
