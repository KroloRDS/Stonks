using MediatR;
using Stonks.Data;
using Stonks.Data.Models;

namespace Stonks.CQRS.Queries.Bankruptcy;

public record GetPublicStocksAmountQuery(Guid StockId)
	: IRequest<GetPublicStocksAmountResponse>;

public class GetPublicStocksAmountQueryHandler :
	IRequestHandler<GetPublicStocksAmountQuery, GetPublicStocksAmountResponse>
{
	private readonly AppDbContext _ctx;

	public GetPublicStocksAmountQueryHandler(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public async Task<GetPublicStocksAmountResponse> Handle(
		GetPublicStocksAmountQuery request,
		CancellationToken cancellationToken)
	{
		var stock = await _ctx.GetById<Stock>(request.StockId);
		return new GetPublicStocksAmountResponse(
			stock.PublicallyOfferredAmount);
	}
}
