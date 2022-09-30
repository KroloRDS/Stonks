using MediatR;
using Stonks.Data;
using Stonks.Models;

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
		await Task.Run(() => _ctx.TradeOffer.RemoveRange(
			_ctx.TradeOffer.Where(x => x.StockId == request.StockId)),
			cancellationToken);
		return Unit.Value;
	}
}
