using Stonks.Data;
using Stonks.Models;
using Stonks.Requests.Commands.Bankruptcy;

namespace Stonks.Handlers.Commands.Bankruptcy;

public class BankruptHandler : ICommandHandler<BankruptCommand>
{
	private readonly AppDbContext _ctx;
	private readonly ICommandHandler<RemoveAllOffersForStockCommand> _removeOffersHandler;
	private readonly ICommandHandler<RemoveAllSharesCommand> _removeSharesHandler;

	public BankruptHandler(AppDbContext ctx, 
		ICommandHandler<RemoveAllOffersForStockCommand> removeOffersHandler,
		ICommandHandler<RemoveAllSharesCommand> removeSharesHandler)
	{
		_ctx = ctx;
		_removeOffersHandler = removeOffersHandler;
		_removeSharesHandler = removeSharesHandler;
	}

	public void Handle(BankruptCommand command)
	{
		var stock = _ctx.GetById<Stock>(command.StockId);
		stock.Bankrupt = true;
		stock.BankruptDate = DateTime.Now;
		stock.PublicallyOfferredAmount = 0;

		_removeOffersHandler.Handle(new RemoveAllOffersForStockCommand(command.StockId));
		_removeSharesHandler.Handle(new RemoveAllSharesCommand(command.StockId));
	}
}
