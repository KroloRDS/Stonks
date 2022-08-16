using Stonks.Data;
using Stonks.Models;
using Stonks.Requests.Commands.Bankruptcy;

namespace Stonks.Handlers.Commands.Bankruptcy;

public class RemoveAllSharesHandler : ICommandHandler<RemoveAllSharesCommand>
{
	private readonly AppDbContext _ctx;

	public RemoveAllSharesHandler(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public void Handle(RemoveAllSharesCommand command)
	{
		var id = _ctx.EnsureExist<Stock>(command?.StockId);
		_ctx.RemoveRange(_ctx.Share.Where(x => x.StockId == id));
	}
}
