using MediatR;
using Microsoft.EntityFrameworkCore;

using Stonks.Util;
using Stonks.Data;
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
            .Select(x => x.Id)
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

        var test = 1 - (Normalise(5, 5, 0) ?? 0);
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
