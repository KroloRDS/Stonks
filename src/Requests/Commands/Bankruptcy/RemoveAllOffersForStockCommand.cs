using MediatR;

namespace Stonks.Requests.Commands.Bankruptcy;

public record RemoveAllOffersForStockCommand(Guid StockId) : IRequest;

public class RemoveAllOffersForStockCommandHandler :
	IRequestHandler<RemoveAllOffersForStockCommand>
{
	public Task<Unit> Handle(RemoveAllOffersForStockCommand request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
