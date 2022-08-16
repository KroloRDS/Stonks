using Stonks.Contracts.Queries.Bankruptcy;
using Stonks.Data;

namespace Stonks.Handlers.Queries.Bankruptcy;

public class GetTotalAmountOfSharesHandler :
	QueryHandler<GetTotalAmountOfSharesQuery, GetTotalAmountOfSharesResponse>
{
	private readonly AppDbContext _ctx;

	public GetTotalAmountOfSharesHandler(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public override GetTotalAmountOfSharesResponse AfterValidate(
		GetTotalAmountOfSharesQuery query)
	{
		var amounts = _ctx.Share
			.Where(x => x.StockId == query.StockId)
			.Select(x => x.Amount);

		var amount = amounts.Any() ? amounts.Sum() : 0;
		return new GetTotalAmountOfSharesResponse(amount);
	}
}
