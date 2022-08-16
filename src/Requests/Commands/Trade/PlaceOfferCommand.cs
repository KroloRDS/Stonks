using MediatR;
using Stonks.Models;

namespace Stonks.Requests.Commands.Trade;

public record PlaceOfferCommand(
	Guid StockId,
	Guid WriterId,
	int Amount,
	OfferType Type,
	decimal Price
) : IRequest;

public class PlaceOfferCommandHandler :
	IRequestHandler<PlaceOfferCommand>
{
	public Task<Unit> Handle(PlaceOfferCommand request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
