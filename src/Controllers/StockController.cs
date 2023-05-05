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
	[Route("stocks")]
	public async Task<GetStocksViewModelResponse> GetStocks(
		CancellationToken cancellationToken)
	{
		var userId = GetUserId();
		if (userId is null) throw new Exception("Unauthorised");
		return await TryGetViewModel(new GetStocksViewModelQuery(
			userId.Value), cancellationToken);
	}

	[HttpGet]
	[AllowAnonymous]
	[Route("echo")]
	public async Task<string> Echo(string message,
		CancellationToken cancellationToken)
	{
		//TODO: add user controller
		//https://www.endpointdev.com/blog/2022/06/implementing-authentication-in-asp.net-core-web-apis/
		return message;
	}
}
