using MediatR;
using Microsoft.EntityFrameworkCore;
using Stonks.Data;
using Stonks.Models;
using Stonks.Responses.Common;

namespace Stonks.Requests.Queries.Common;

public class GetHistoricalPricesQuery : IRequest<GetHistoricalPricesResponse>
{
	public Guid StockId { get; set; }
	public DateTime FromDate { get; set; }
	public DateTime? ToDate { get; set; }

	public void Validate()
	{
		if (StockId == default)
			throw new ArgumentNullException(nameof(StockId));

		if (FromDate == default)
			throw new ArgumentNullException(nameof(FromDate));

		if (ToDate is not null && ToDate <= FromDate)
			throw new ArgumentOutOfRangeException(nameof(ToDate));
	}
}

public class GetHistoricalPricesQueryHandler :
	IRequestHandler<GetHistoricalPricesQuery, GetHistoricalPricesResponse>
{
	private readonly AppDbContext _ctx;

	public GetHistoricalPricesQueryHandler(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public async Task<GetHistoricalPricesResponse> Handle(
		GetHistoricalPricesQuery request, CancellationToken cancellationToken)
	{
		request.Validate();
		var toDate = request.ToDate ?? DateTime.MaxValue;

		var list = await _ctx.AvgPrice
			.Where(x => x.StockId == request.StockId &&
				x.DateTime >= request.FromDate &&
				x.DateTime <= toDate)
			.OrderBy(x => x.DateTime)
			.ToListAsync(cancellationToken);

		return new GetHistoricalPricesResponse(list);
	}
}
