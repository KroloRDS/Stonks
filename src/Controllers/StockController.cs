using MediatR;
using Microsoft.AspNetCore.Mvc;

using Stonks.Data;
using Stonks.ViewModels;
using Stonks.Managers;
using Stonks.Requests.Queries.Common;

namespace Stonks.Controllers;

public class StockController : BaseController
{
	public StockController(IMediator mediator, ILogManager logger,
		AppDbContext context) : base(mediator, logger, context)
	{
	}

	[Route("{stockSymbol}")]
	public async Task<IActionResult> Index(string stockSymbol,
		CancellationToken cancellationToken)
	{
		return await TryGetViewModel(new GetStockViewModelQuery(
			stockSymbol, Guid.NewGuid()), cancellationToken);
	}
}
