using MediatR;
using Microsoft.EntityFrameworkCore;
using Stonks.Data;

namespace Stonks.CQRS.Queries.Bankruptcy;

public record GetTotalAmountOfSharesQuery(Guid StockId)
	: IRequest<GetTotalAmountOfSharesResponse>;

public class GetTotalAmountOfSharesQueryHandler :
	IRequestHandler<GetTotalAmountOfSharesQuery, GetTotalAmountOfSharesResponse>
{
	private readonly AppDbContext _ctx;

	public GetTotalAmountOfSharesQueryHandler(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public async Task<GetTotalAmountOfSharesResponse> Handle(
		GetTotalAmountOfSharesQuery request,
		CancellationToken cancellationToken)
	{
		var sum = await _ctx.Share
			.Where(x => x.StockId == request.StockId)
			.SumAsync(x => x.Amount, cancellationToken);
		return new GetTotalAmountOfSharesResponse(sum);
	}
}
