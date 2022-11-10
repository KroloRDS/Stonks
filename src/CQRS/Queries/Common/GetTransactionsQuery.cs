using MediatR;
using Microsoft.EntityFrameworkCore;
using Stonks.Data;
using Stonks.Data.Models;

namespace Stonks.CQRS.Queries.Common;

public record GetTransactionsQuery : IRequest<GetTransactionsResponse>
{
	public Guid? StockId { get; set; }
	public Guid? UserId { get; set; }
	public DateTime? FromDate { get; set; }
}

public class GetTransactionsQueryHandler :
	BaseQuery<GetTransactionsQuery, GetTransactionsResponse>
{
	public GetTransactionsQueryHandler(ReadOnlyDbContext ctx) : base(ctx) {}

	public override async Task<GetTransactionsResponse> Handle(
		GetTransactionsQuery request, CancellationToken cancellationToken)
	{
		var queryStock = (Transaction x) => !request.StockId.HasValue ||
			x.StockId == request.StockId;

		var queryUser = (Transaction x) => !request.UserId.HasValue ||
			x.BuyerId == request.UserId || x.SellerId == request.UserId;

		var queryFrom = (Transaction x) => !request.FromDate.HasValue ||
			x.Timestamp >= request.FromDate;

		var query = (Transaction x) => queryStock(x) &&
			queryUser(x) && queryFrom(x);

		var list = await Task.Run(() => _ctx.Transaction
			.Where(query)
			.Select(x => new Transaction
			{
				Amount = x.Amount,
				Price = x.Price
			}).ToList(), cancellationToken);

		return new GetTransactionsResponse(list);
	}
}
