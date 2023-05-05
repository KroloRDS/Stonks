using MediatR;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Stonks.Data;
using Stonks.Util;
using Microsoft.EntityFrameworkCore;

namespace Stonks.Controllers;

public class BaseController : Controller
{
	protected readonly IMediator _mediator;
	private readonly IStonksLogger _logger;
	protected readonly AppDbContext _context;

	public BaseController(IMediator mediator,
        IStonksLogger logger, AppDbContext context)
	{
		_mediator = mediator;
		_logger = logger;
		_context = context;
	}

	protected async Task<IActionResult> TryExecuteCommand(
		IRequest request, CancellationToken cancellationToken)
	{
		try
		{
			await _mediator.Send(request, cancellationToken);
			return Ok("Success");
		}
		catch (Exception ex)
		{
			_logger.Log(ex, request);
			return Problem("Internal Server Error");
		}
	}

	protected async Task<IActionResult> TryGetViewModel<T>(
		IRequest<T> request, CancellationToken cancellationToken)
		where T : class
	{
		try
		{
			var result = await _mediator.Send(request, cancellationToken);
			return View(result);
		}
		catch (Exception ex)
		{
			_logger.Log(ex, request);
			return Problem("Internal Server Error");
		}
	}

	protected Guid? GetUserId()
	{
		var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
		return id is null ? default : Guid.Parse(id);
	}

	protected async Task<Guid> GetStockId(string symbol,
		CancellationToken cancellationToken)
	{
		var stock = await _context.Stock
			.FirstAsync(x => x.Symbol == symbol, cancellationToken);
		return stock.Id;
	}
}
