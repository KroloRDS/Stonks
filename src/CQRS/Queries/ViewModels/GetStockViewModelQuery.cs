using MediatR;
using Microsoft.EntityFrameworkCore;
using Stonks.CQRS.Queries.Common;
using Stonks.Data;

namespace Stonks.CQRS.Queries.ViewModels;

public record GetStockViewModelQuery(string StockSymbol, Guid UserId)
    : IRequest<GetStockViewModelResponse>;

public class GetStockViewModelQueryHandler :
    IRequestHandler<GetStockViewModelQuery, GetStockViewModelResponse>
{
    private readonly AppDbContext _ctx;
    private readonly IMediator _mediator;

    public GetStockViewModelQueryHandler(AppDbContext ctx, IMediator mediator)
    {
        _ctx = ctx;
        _mediator = mediator;
    }

    public async Task<GetStockViewModelResponse> Handle(GetStockViewModelQuery request,
        CancellationToken cancellationToken)
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
