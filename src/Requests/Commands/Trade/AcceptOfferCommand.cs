using MediatR;

namespace Stonks.Requests.Commands.Trade;

public record AcceptOfferCommand(Guid UserId, Guid OfferId) : IRequest;

public class AcceptOfferCommandHandler : IRequestHandler<AcceptOfferCommand>
{
	public Task<Unit> Handle(AcceptOfferCommand request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
