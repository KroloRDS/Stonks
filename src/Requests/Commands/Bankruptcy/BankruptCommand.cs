using MediatR;

namespace Stonks.Requests.Commands.Bankruptcy;

public record BankruptCommand(Guid StockId) : IRequest;

public class BankruptCommandHandler : IRequestHandler<AddPublicOffersCommand>
{
	public Task<Unit> Handle(AddPublicOffersCommand request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
