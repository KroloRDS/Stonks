using MediatR;
using Microsoft.EntityFrameworkCore;

using Stonks.Util;
using Stonks.Data;
using Stonks.Data.Models;
using Stonks.CQRS.Queries.Common;

namespace Stonks.CQRS.Queries.Bankruptcy.GetWeakestStock;

public record GetWeakestStockIdQuery : IRequest<GetWeakestStockIdResponse>;

public class GetWeakestStockIdQueryHandler :
    BaseQuery<GetWeakestStockIdQuery, GetWeakestStockIdResponse>
{
    private readonly IStonksConfiguration _config;

    public GetWeakestStockIdQueryHandler(ReadOnlyDbContext ctx,
        IMediator mediator, IStonksConfiguration config) : base(ctx, mediator)
    {
        _config = config;
    }

    public override async Task<GetWeakestStockIdResponse> Handle(
        GetWeakestStockIdQuery request, CancellationToken cancellationToken)
    {
        var stocks = await _ctx.Stock.Where(x => !x.Bankrupt)
            .ToListAsync(cancellationToken);

        if (!stocks.Any())
            throw new NoStocksToBankruptException();

        var indicators = await Task.WhenAll(stocks
            .Select(x => GetStockIndicator(x, cancellationToken)));

        var worstStock = Normalise(indicators)
            .OrderBy(x => GetScore(x)).First();

        return new GetWeakestStockIdResponse(worstStock.StockId);
    }

    private async Task<StockIndicator> GetStockIndicator(
        Stock stock, CancellationToken cancellationToken)
    {
        var publicStocks = _mediator.Send(
            new GetPublicStocksAmountQuery(stock.Id), cancellationToken);
        var marketCap = GetMarketCap(stock, cancellationToken);
        var volatility = GetVolatility(stock.Id, cancellationToken);

        return new StockIndicator
        {
            StockId = stock.Id,
            MarketCap = await marketCap,
            StocksAmount = (await publicStocks).Amount,
            Volatility = await volatility,
            Fun = new Random().NextDouble()
        };
    }

    private async Task<decimal> GetMarketCap(
		Stock stock, CancellationToken cancellationToken)
    {
        var shares = _mediator.Send(new GetTotalAmountOfSharesQuery
			(stock.Id), cancellationToken);
        return (await shares).Amount * stock.Price;
    }

    private async Task<double> GetVolatility(
        Guid stockId, CancellationToken cancellationToken)
    {
        var lastBankruptDate = await _mediator.Send(
            new GetLastBankruptDateQuery(), cancellationToken);

        var result = await _mediator.Send(new GetStockPricesQuery
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

    public static IEnumerable<StockIndicatorNormalised> Normalise(
        IEnumerable<StockIndicator>? indicators)
    {
        if (indicators is null || !indicators.Any())
            return Enumerable.Empty<StockIndicatorNormalised>();

        var minMarketCap = indicators.Min(x => x.MarketCap);
        var marketCapMaxDiff = indicators.Max(x => x.MarketCap) - minMarketCap;

        var minVolatility = indicators.Min(x => x.Volatility);
        var volatilityMaxDiff = indicators.Max(x => x.Volatility) - minVolatility;

        var minStocksAvailable = indicators.Min(x => x.StocksAmount);
        var stocksAvailableMaxDiff = indicators.Max(x => x.StocksAmount) - minStocksAvailable;

        return indicators.Select(x => new StockIndicatorNormalised
        {
            StockId = x.StockId,
            Fun = x.Fun,
            MarketCap = Normalise((double)x.MarketCap, (double)minMarketCap, (double)marketCapMaxDiff) ?? 1,
            StocksAmount = 1 - (Normalise(x.StocksAmount, minStocksAvailable, stocksAvailableMaxDiff) ?? 0),
            Volatility = 1 - (Normalise(x.Volatility, minVolatility, volatilityMaxDiff) ?? 0)
        });
    }

    private static double? Normalise(double x, double min, double maxDiff)
    {
        if (maxDiff == 0) return null;
        return (x - min) / maxDiff;
    }
}
