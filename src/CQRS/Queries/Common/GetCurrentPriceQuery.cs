using MediatR;
using Microsoft.EntityFrameworkCore;
using Stonks.Util;
using Stonks.Data;
using Stonks.Data.Models;

namespace Stonks.CQRS.Queries.Common;

public record GetCurrentPriceQuery(Guid StockId)
	: IRequest<GetCurrentPriceResponse>;

public class GetCurrentPriceQueryHandler :
	BaseQuery<GetCurrentPriceQuery, GetCurrentPriceResponse>
{
	public GetCurrentPriceQueryHandler(ReadOnlyDbContext ctx) : base(ctx) {}

	public override async Task<GetCurrentPriceResponse> Handle(
		GetCurrentPriceQuery request, CancellationToken cancellationToken)
	{
		var stock = await _ctx.GetById<Stock>(request.StockId);
		if (stock.Bankrupt) throw new BankruptStockException();

		var price = await _ctx.AvgPriceCurrent.SingleAsync(
			x => x.StockId == request.StockId, cancellationToken);

		return new GetCurrentPriceResponse(price.Price);
	}
}
