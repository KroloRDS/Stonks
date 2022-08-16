using MediatR;
using Stonks.Responses.Bankruptcy;

namespace Stonks.Requests.Queries.Bankruptcy;

public record GetWeakestStockIdQuery : IRequest<GetWeakestStockIdResponse>;

public class GetWeakestStockIdQueryHandler :
	IRequestHandler<GetWeakestStockIdQuery, GetWeakestStockIdResponse>
{
	public Task<GetWeakestStockIdResponse> Handle(GetWeakestStockIdQuery request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
