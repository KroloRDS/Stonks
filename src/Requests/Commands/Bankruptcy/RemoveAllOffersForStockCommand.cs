using MediatR;
using Stonks.Data;
using Stonks.Models;
using Z.EntityFramework.Plus;

namespace Stonks.Requests.Commands.Bankruptcy;

public record RemoveAllOffersForStockCommand(Guid StockId) : IRequest;

public class RemoveAllOffersForStockCommandHandler :
	IRequestHandler<RemoveAllOffersForStockCommand>
{
	private readonly AppDbContext _ctx;

	public RemoveAllOffersForStockCommandHandler(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public async Task<Unit> Handle(RemoveAllOffersForStockCommand request,
		CancellationToken cancellationToken)
	{
		await _ctx.EnsureExistAsync<Stock>(request.StockId, cancellationToken);
		await _ctx.TradeOffer.Where(x => x.StockId == request.StockId)
			.DeleteAsync(cancellationToken);
		return Unit.Value;
	}
}
