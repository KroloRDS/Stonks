using MediatR;
using Stonks.Responses.Bankruptcy;

namespace Stonks.Requests.Queries.Bankruptcy;

public record GetLastBankruptDateQuery : IRequest<GetLastBankruptDateResponse>;

public class GetLastBankruptDateQueryHandler :
	IRequestHandler<GetLastBankruptDateQuery, GetLastBankruptDateResponse>
{
	public Task<GetLastBankruptDateResponse> Handle(GetLastBankruptDateQuery request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
