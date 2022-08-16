using MediatR;

namespace Stonks.Requests.Commands.Bankruptcy;

public record AddPublicOffersCommand(int Amount) : IRequest;

public class AddPublicOffersCommandHandler : IRequestHandler<AddPublicOffersCommand>
{
	public Task<Unit> Handle(AddPublicOffersCommand request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
