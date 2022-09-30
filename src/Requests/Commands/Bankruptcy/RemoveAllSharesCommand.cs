using MediatR;
using Stonks.Data;
using Stonks.Models;

namespace Stonks.Requests.Commands.Bankruptcy;

public record RemoveAllSharesCommand(Guid StockId) : IRequest;

public class RemoveAllSharesCommandHandler :
	IRequestHandler<RemoveAllSharesCommand>
{
	private readonly AppDbContext _ctx;

	public RemoveAllSharesCommandHandler(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public async Task<Unit> Handle(RemoveAllSharesCommand request,
		CancellationToken cancellationToken)
	{
		var id = await _ctx.EnsureExistAsync<Stock>(
			request?.StockId, cancellationToken);
		await Task.Run(() => _ctx.Share.RemoveRange(
			_ctx.Share.Where(x => x.StockId == id)), cancellationToken);
		return Unit.Value;
	}
}
