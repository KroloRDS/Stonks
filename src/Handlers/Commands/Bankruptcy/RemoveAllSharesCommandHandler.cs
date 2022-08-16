using Stonks.Contracts.Commands.Bankruptcy;
using Stonks.Data;
using Stonks.Models;

namespace Stonks.Handlers.Commands.Bankruptcy;

public class RemoveAllSharesCommandHandler : ICommandHandler<RemoveAllSharesCommand>
{
	private readonly AppDbContext _ctx;

	public RemoveAllSharesCommandHandler(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public void Handle(RemoveAllSharesCommand command)
	{
		var id = _ctx.EnsureExist<Stock>(command?.StockId);
		_ctx.RemoveRange(_ctx.Share.Where(x => x.StockId == id));
	}
}
