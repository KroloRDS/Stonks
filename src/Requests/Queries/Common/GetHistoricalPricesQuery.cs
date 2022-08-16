using MediatR;
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
		{
			throw new ArgumentNullException(nameof(StockId));
		}

		if (FromDate == default)
		{
			throw new ArgumentNullException(nameof(FromDate));
		}
	}
}

public class GetHistoricalPricesQueryHandler :
	IRequestHandler<GetHistoricalPricesQuery, GetHistoricalPricesResponse>
{
	public Task<GetHistoricalPricesResponse> Handle(GetHistoricalPricesQuery request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
