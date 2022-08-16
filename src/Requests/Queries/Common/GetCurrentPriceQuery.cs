using MediatR;
using Stonks.Responses.Common;

namespace Stonks.Requests.Queries.Common;

public class GetCurrentPriceQuery : IRequest<GetCurrentPriceResponse>
{
	public Guid StockId { get; set; }

	public void Validate()
	{
		if (StockId == default)
		{
			throw new ArgumentNullException(nameof(StockId));
		}
	}
}

public class GetCurrentPriceQueryHandler :
	IRequestHandler<GetCurrentPriceQuery, GetCurrentPriceResponse>
{
	public Task<GetCurrentPriceResponse> Handle(GetCurrentPriceQuery request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
