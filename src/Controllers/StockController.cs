using MediatR;
using Microsoft.AspNetCore.Mvc;

using Stonks.Data;
using Stonks.Providers;
using Stonks.Requests.Queries.Common;

namespace Stonks.Controllers;

public class StockController : BaseController
{
	public StockController(IMediator mediator, IStonksLogger logger,
		AppDbContext context) : base(mediator, logger, context) {}

	[Route("stock/{stockSymbol}")]
	public async Task<IActionResult> Index(string stockSymbol,
		CancellationToken cancellationToken)
	{
		return await TryGetViewModel(new GetStockViewModelQuery(
			stockSymbol, Guid.NewGuid()), cancellationToken);
	}
}
