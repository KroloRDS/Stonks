using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Stonks.Util;
using Stonks.Data;
using Stonks.CQRS.Queries.Common;
using Stonks.Data.Models;

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
		var userId = GetUserId();
		if (userId is null) throw new Exception("Unauthorised");
		return await TryGetViewModel(new GetStocksViewModelQuery(
			userId.Value), cancellationToken);
	}

	//Methods for debug purposes
	//TODO: add user controller
	//https://www.endpointdev.com/blog/2022/06/implementing-authentication-in-asp.net-core-web-apis/
	[HttpGet]
	[AllowAnonymous]
	[Route("echo")]
	public async Task<string> Echo(string message,
		CancellationToken cancellationToken)
	{
		return message;
	}

	[HttpGet]
	[AllowAnonymous]
	[Route("getStocks")]
	public async Task<IEnumerable<Stock>> GetStocks(
		CancellationToken cancellationToken)
	{
		return _context.Stock.ToList();
	}
}
