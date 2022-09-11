using MediatR;
using Microsoft.EntityFrameworkCore;
using Stonks.Data;
using Stonks.Helpers;
using Stonks.Models;
using Stonks.Responses.Common;

namespace Stonks.Requests.Queries.Common;

public record GetCurrentPriceQuery(Guid StockId)
	: IRequest<GetCurrentPriceResponse>;

public class GetCurrentPriceQueryHandler :
	IRequestHandler<GetCurrentPriceQuery, GetCurrentPriceResponse>
{
	private readonly AppDbContext _ctx;

	public GetCurrentPriceQueryHandler(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public async Task<GetCurrentPriceResponse> Handle(
		GetCurrentPriceQuery request, CancellationToken cancellationToken)
	{
		var stock = await _ctx.GetByIdAsync<Stock>(request.StockId);

		if (stock.Bankrupt)
			throw new BankruptStockException();

		var price = await _ctx.AvgPriceCurrent.SingleAsync(
			x => x.StockId == request.StockId, cancellationToken);

		return new GetCurrentPriceResponse(price.Amount);
	}
}
