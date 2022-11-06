using MediatR;
using Microsoft.EntityFrameworkCore;
using Stonks.Data;

namespace Stonks.CQRS.Queries.Bankruptcy;

public record GetLastBankruptDateQuery : IRequest<GetLastBankruptDateResponse>;

public class GetLastBankruptDateQueryHandler :
	BaseQuery<GetLastBankruptDateQuery, GetLastBankruptDateResponse>
{
	public GetLastBankruptDateQueryHandler(
		ReadOnlyDbContext ctx) : base(ctx) {}

	public override async Task<GetLastBankruptDateResponse> Handle(
		GetLastBankruptDateQuery request, CancellationToken cancellationToken)
	{
		var stocks = _ctx.Stock.Where(x => x.BankruptDate.HasValue)
			.Select(x => x.BankruptDate!.Value);

		var hasBankrupted = await stocks.AnyAsync(cancellationToken);
		DateTime? date = hasBankrupted ?
			await stocks.MaxAsync(cancellationToken) : null;
		return new GetLastBankruptDateResponse(date);
	}
}
