using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Stonks.Util;
using Stonks.Data;
using Stonks.Data.Models;
using Stonks.Views.Models;
using Stonks.CQRS.Queries.Common;

namespace Stonks.Controllers;

[Authorize]
public class StockController : BaseController
{
	public StockController(IMediator mediator, IStonksLogger logger,
		AppDbContext context) : base(mediator, logger, context) {}

	[Route("stocks")]
	public async Task<IActionResult> Index(
		CancellationToken cancellationToken)
	{
		var userId = GetUserId();
		if (userId is null) return Problem("Unauthorised");
		return await TryGetViewModel(new GetStocksViewModelQuery(
			userId.Value), cancellationToken);
	}

	[Route("stocks/{symbol}")]
	public async Task<IActionResult> Stock(string symbol,
		CancellationToken cancellationToken)
	{
		//TODO: Pass model from previous view instead of querying again
		var userId = GetUserId();
		if (userId is null) return Problem("Unauthorised");
		var stockId = await GetStockId(symbol, cancellationToken);
		var models = await _mediator.Send(
			new GetStocksViewModelQuery(userId.Value), cancellationToken);
		return View(models.Stocks.First(x => x.Id == stockId));
	}
}
