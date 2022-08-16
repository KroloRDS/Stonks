using MediatR;

namespace Stonks.Requests.Commands.Bankruptcy;

public record EmitNewSharesCommand(int Amount) : IRequest;

public class EmitNewSharesCommandHandler :
	IRequestHandler<EmitNewSharesCommand>
{
	public Task<Unit> Handle(EmitNewSharesCommand request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
