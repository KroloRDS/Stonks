using MediatR;
using Stonks.Data;
using Stonks.Data.Models;

namespace Stonks.CQRS.Queries.Common;

public class GetStockPricesQuery : IRequest<GetStockPricesResponse>
{
	public Guid? StockId { get; set; }
	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }

	public void Validate()
	{
		if (FromDate is not null &&
			ToDate is not null && 
			ToDate <= FromDate)
			throw new ArgumentOutOfRangeException(nameof(ToDate));
	}
}

public class GetStockPricesQueryHandler :
	BaseQuery<GetStockPricesQuery, GetStockPricesResponse>
{
	public GetStockPricesQueryHandler(ReadOnlyDbContext ctx) : base(ctx) {}

	public override async Task<GetStockPricesResponse> Handle(
		GetStockPricesQuery request, CancellationToken cancellationToken)
	{
		request.Validate();

		var query = (AvgPrice x) => !request.StockId.HasValue || 
			x.StockId == request.StockId;

		var queryFrom = request.FromDate is null ? query :
			(AvgPrice x) => query(x) && x.DateTime >= request.FromDate;

		var queryTo = request.ToDate is null ? queryFrom :
			(AvgPrice x) => queryFrom(x) && x.DateTime <= request.ToDate;

		var prices = await Task.Run(() => _ctx.AvgPrice
			.Where(queryTo)
			.OrderBy(x => x.DateTime)
			.ToList(), cancellationToken);

		return new GetStockPricesResponse(prices);
	}
}
