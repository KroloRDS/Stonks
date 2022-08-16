using MediatR;
using Stonks.Responses.Bankruptcy;

namespace Stonks.Requests.Queries.Bankruptcy;

public class GetPublicStocksAmountQuery : IRequest<GetPublicStocksAmountResponse>
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

public class GetPublicStocksAmountQueryHandler :
	IRequestHandler<GetPublicStocksAmountQuery, GetPublicStocksAmountResponse>
{
	public Task<GetPublicStocksAmountResponse> Handle(GetPublicStocksAmountQuery request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
