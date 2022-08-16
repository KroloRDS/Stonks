using MediatR;

namespace Stonks.Requests.Commands.Trade;

public record CancelOfferCommand(Guid OfferId) : IRequest;

public class CancelOfferCommandHandler :
	IRequestHandler<CancelOfferCommand>
{
	public Task<Unit> Handle(CancelOfferCommand request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
