using MediatR;
using Stonks.Responses.Common;

namespace Stonks.Requests.Queries.Common;

public class GetTransactionsQuery : IRequest<GetTransactionsResponse>
{
	Guid StockId { get; set; }
	DateTime? From { get; set; }

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
	public Task<GetTransactionsResponse> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
