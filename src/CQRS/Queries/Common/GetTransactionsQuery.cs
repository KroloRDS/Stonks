using MediatR;
using Stonks.Data;
using Stonks.Data.Models;

namespace Stonks.CQRS.Queries.Common;

public record GetTransactionsQuery(Guid StockId, DateTime? FromDate = null)
	: IRequest<GetTransactionsResponse>;

public class GetTransactionsQueryHandler :
	BaseQuery<GetTransactionsQuery, GetTransactionsResponse>
{
	public GetTransactionsQueryHandler(ReadOnlyDbContext ctx) : base(ctx) {}

	public override async Task<GetTransactionsResponse> Handle(
		GetTransactionsQuery request, CancellationToken cancellationToken)
	{
		await _ctx.EnsureExist<Stock>(request.StockId, cancellationToken);
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
