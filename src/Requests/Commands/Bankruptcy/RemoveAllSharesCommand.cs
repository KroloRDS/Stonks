using MediatR;

namespace Stonks.Requests.Commands.Bankruptcy;

public record RemoveAllSharesCommand(Guid StockId) : IRequest;

public class RemoveAllSharesCommandHandler :
	IRequestHandler<RemoveAllSharesCommand>
{
	public Task<Unit> Handle(RemoveAllSharesCommand request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
