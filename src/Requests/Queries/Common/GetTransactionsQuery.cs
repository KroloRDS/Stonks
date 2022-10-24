using MediatR;
using Stonks.Data;
using Stonks.Models;
using Stonks.Responses.Common;

namespace Stonks.Requests.Queries.Common;

public record GetTransactionsQuery(Guid StockId, DateTime? FromDate = null)
	: IRequest<GetTransactionsResponse>;

public class GetTransactionsQueryHandler :
	IRequestHandler<GetTransactionsQuery, GetTransactionsResponse>
{
	private readonly AppDbContext _ctx;

	public GetTransactionsQueryHandler(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public async Task<GetTransactionsResponse> Handle(
		GetTransactionsQuery request, CancellationToken cancellationToken)
	{
		await _ctx.EnsureExistAsync<Stock>(request.StockId, cancellationToken);
		var query = (Transaction x) => x.StockId == request.StockId;

		var queryFrom = request.FromDate is null ? query :
			(Transaction x) => query(x) && x.Timestamp >= request.FromDate;

		var list = await Task.Run(() => _ctx.Transaction
			.Where(queryFrom)
			.Select(x => new Transaction
			{
				Amount = x.Amount,
				Price = x.Price
			})
			.ToList(), cancellationToken);

		return new GetTransactionsResponse(list);
	}
}
