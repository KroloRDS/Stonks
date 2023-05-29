using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Stonks.Util;
using Stonks.Data;
using Stonks.CQRS.Queries.Common;

namespace Stonks.Controllers;

[Authorize]
public class StockController : BaseController
{
	public StockController(IMediator mediator, IStonksLogger logger,
		AppDbContext context) : base(mediator, logger, context) {}

	[HttpGet]
	[Route("myStocks")]
	public async Task<GetStocksViewModelResponse> MyStocks(
		CancellationToken cancellationToken)
	{
		var userId = GetUserId() ?? throw new Exception("Unauthorised");
		return await TryGetViewModel(new GetStocksViewModelQuery(
			userId), cancellationToken);
	}
}
