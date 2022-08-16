using MediatR;
using Microsoft.EntityFrameworkCore;
using Stonks.Data;
using Stonks.Helpers;
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
		var date = hasBankrupted ?
			await stocks.MaxAsync(cancellationToken) : 
			GlobalConstants.BEGIN_DATE;
		return new GetLastBankruptDateResponse(date);
	}
}
