using MediatR;
using Microsoft.EntityFrameworkCore;
using Stonks.Data;
using Stonks.CQRS.Queries.Common;

namespace Stonks.CQRS.Queries.ViewModels;

public record GetStockViewModelQuery(string StockSymbol, Guid UserId)
    : IRequest<GetStockViewModelResponse>;

public class GetStockViewModelQueryHandler :
    BaseQuery<GetStockViewModelQuery, GetStockViewModelResponse>
{
    public GetStockViewModelQueryHandler(
		ReadOnlyDbContext ctx, IMediator mediator) : base(ctx, mediator) {}

    public override async Task<GetStockViewModelResponse> Handle(
		GetStockViewModelQuery request, CancellationToken cancellationToken)
    {
        var stock = await _ctx.Stock.FirstAsync(
            x => x.Symbol == request.StockSymbol, cancellationToken);

        var historical = _mediator.Send(new GetHistoricalPricesQuery
        {
            StockId = stock.Id,
            FromDate = DateTime.Now.AddMonths(-1)
        }, cancellationToken);

        var current = _mediator.Send(new GetCurrentPriceQuery(stock.Id),
            cancellationToken);

        return new GetStockViewModelResponse(stock, (await historical).Prices,
            (await current).Price);
    }
}
