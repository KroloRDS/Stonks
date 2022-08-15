using Microsoft.AspNetCore.Mvc;

using Stonks.Data;
using Stonks.ViewModels;
using Stonks.Managers.Common;

namespace Stonks.Controllers;

public class StockController : Controller
{
	private readonly AppDbContext _context;
	private readonly IGetPriceManager _priceManager;
	private readonly ILogManager _logger;

	public StockController(AppDbContext context,
		IGetPriceManager priceManager,
		ILogManager logger)
	{
		_context = context;
		_priceManager = priceManager;
		_logger = logger;
	}

	[Route("Stock/{stockSymbol}")]
	public IActionResult Index(string stockSymbol)
	{
		try
		{
			return View(GetStockViewModel(stockSymbol));
		}
		catch (Exception ex)
		{
			_logger.Log(ex, stockSymbol);
			return Problem("Oof");
		}
	}

	private StockViewModel GetStockViewModel(string stockSymbol)
	{
		var stock = _context.Stock
			.First(x => x.Symbol == stockSymbol);

		var prices = _priceManager.GetHistoricalPrices(
			stock.Id, DateTime.Now.AddMonths(-1));

		return new StockViewModel(stock, prices,
			_priceManager.GetCurrentPrice(stock.Id));
	}
}
