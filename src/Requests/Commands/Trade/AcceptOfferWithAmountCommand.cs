using MediatR;

namespace Stonks.Requests.Commands.Trade;

public record AcceptOfferWithAmountCommand(
	Guid UserId, Guid OfferId, decimal Amount) : IRequest;

public class AcceptOfferWithAmountCommandHandler :
	IRequestHandler<AcceptOfferWithAmountCommand>
{
	public Task<Unit> Handle(AcceptOfferWithAmountCommand request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
