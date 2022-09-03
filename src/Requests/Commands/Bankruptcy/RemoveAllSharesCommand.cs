using MediatR;
using Stonks.Data;
using Stonks.Models;
using Z.EntityFramework.Plus;

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
		await _ctx.Share.Where(x => x.StockId == id)
			.DeleteAsync(cancellationToken);
		return Unit.Value;
	}
}
