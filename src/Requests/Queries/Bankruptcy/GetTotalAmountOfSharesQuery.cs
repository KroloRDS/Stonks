using MediatR;
using Stonks.Responses.Bankruptcy;

namespace Stonks.Requests.Queries.Bankruptcy;

public class GetTotalAmountOfSharesQuery : IRequest<GetTotalAmountOfSharesResponse>
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

public class GetTotalAmountOfSharesQueryHandler :
	IRequestHandler<GetTotalAmountOfSharesQuery, GetTotalAmountOfSharesResponse>
{
	public Task<GetTotalAmountOfSharesResponse> Handle(GetTotalAmountOfSharesQuery request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
