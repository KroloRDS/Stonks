using MediatR;
using Microsoft.EntityFrameworkCore;
using Stonks.Data;
using Stonks.Helpers;
using Stonks.Models;
using Stonks.Responses.Common;

namespace Stonks.Requests.Queries.Common;

public class GetTransactionsQuery : IRequest<GetTransactionsResponse>
{
	public Guid StockId { get; set; }
	public DateTime? From { get; set; }

	public void Validate()
	{
		if (StockId == default)
		{
			throw new ArgumentNullException(nameof(StockId));
		}
	}
}

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
		request.Validate();
		var fromDate = request.From ?? GlobalConstants.BEGIN_DATE;
		var list = await _ctx.Transaction
			.Where(x => x.StockId == request.StockId &&
				x.Timestamp >= request.From)
			.Select(x => new Transaction
			{
				Amount = x.Amount,
				Price = x.Price
			})
			.ToListAsync(cancellationToken);

		return new GetTransactionsResponse(list);
	}
}
