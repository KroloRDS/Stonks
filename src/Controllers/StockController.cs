using MediatR;
using Microsoft.AspNetCore.Mvc;

using Stonks.Data;
using Stonks.ViewModels;
using Stonks.Managers;
using Stonks.Requests.Queries.Common;
using Stonks.Responses.Common;

namespace Stonks.Controllers;

public class StockController : BaseController
{
	public StockController(IMediator mediator, ILogManager logger,
		AppDbContext context) : base(mediator, logger, context)
	{
	}

	[Route("Stock/{stockSymbol}")]
	public async Task<IActionResult> Index(string stockSymbol,
		CancellationToken cancellationToken)
	{
		var stock = _context.Stock
			.First(x => x.Symbol == stockSymbol);

		var historicalQuery = TryExecuteQuery(
			new GetHistoricalPricesQuery
			{
				StockId = stock.Id,
				FromDate = DateTime.Now.AddMonths(-1)
			}, cancellationToken);
		var currentQuery = TryExecuteQuery(
			new GetCurrentPriceQuery(stock.Id), cancellationToken);

		var (success, historical) = await historicalQuery;
		if (!success) return Problem("Internal Server Error");

		(success, var current) = await currentQuery;
		if (!success) return Problem("Internal Server Error");

		return View(new StockViewModel(stock,
			historical!.Prices, current!.Price));
				
	}
}
