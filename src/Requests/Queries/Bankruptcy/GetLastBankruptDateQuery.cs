using MediatR;
using Microsoft.EntityFrameworkCore;
using Stonks.Data;
using Stonks.Responses.Bankruptcy;

namespace Stonks.Requests.Queries.Bankruptcy;

public record GetLastBankruptDateQuery : IRequest<GetLastBankruptDateResponse>;

public class GetLastBankruptDateQueryHandler :
	IRequestHandler<GetLastBankruptDateQuery, GetLastBankruptDateResponse>
{
	private readonly AppDbContext _ctx;

	public GetLastBankruptDateQueryHandler(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public async Task<GetLastBankruptDateResponse> Handle(
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
