using MediatR;
using Microsoft.EntityFrameworkCore;
using Stonks.Data;

namespace Stonks.CQRS.Queries.Bankruptcy;

public record GetTotalAmountOfSharesQuery(Guid StockId)
	: IRequest<GetTotalAmountOfSharesResponse>;

public class GetTotalAmountOfSharesQueryHandler :
	BaseQuery<GetTotalAmountOfSharesQuery, GetTotalAmountOfSharesResponse>
{
	public GetTotalAmountOfSharesQueryHandler(
		ReadOnlyDbContext ctx) : base(ctx) {}

	public override async Task<GetTotalAmountOfSharesResponse> Handle(
		GetTotalAmountOfSharesQuery request,
		CancellationToken cancellationToken)
	{
		var sum = await _ctx.Share
			.Where(x => x.StockId == request.StockId)
			.SumAsync(x => x.Amount, cancellationToken);
		return new GetTotalAmountOfSharesResponse(sum);
	}
}
