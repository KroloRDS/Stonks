﻿using MediatR;
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
		return await TryGetViewModel(new GetStocksViewModelQuery(GetUserId()),
			cancellationToken);
	}

	[Route("stocks/{symbol}")]
	public async Task<IActionResult> Stock(string symbol,
		CancellationToken cancellationToken)
	{
		//TODO: Pass model from previous view instead of querying again
		var stockId = await GetStockId(symbol, cancellationToken);
		var models = await _mediator.Send(
			new GetStocksViewModelQuery(GetUserId()), cancellationToken);
		return View(models.Stocks.First(x => x.Id == stockId));
	}

	[Route("stocks/{symbol}/buy")]
	public async Task<IActionResult> Buy(string symbol,
		CancellationToken cancellationToken)
	{
		return View("Trade", await GetPlaceOfferViewModel(
			symbol, OfferType.Buy, cancellationToken));
	}

	[Route("stocks/{symbol}/sell")]
	public async Task<IActionResult> Sell(string symbol,
		CancellationToken cancellationToken)
	{
		return View("Trade", await GetPlaceOfferViewModel(
			symbol, OfferType.Sell, cancellationToken));
	}

	private async Task<PlaceOfferViewModel> GetPlaceOfferViewModel(
		string symbol, OfferType type, CancellationToken cancellationToken)
	{
		return new PlaceOfferViewModel
		{
			OfferType = type,
			StockId = await GetStockId(symbol, cancellationToken),
			StockSymbol = symbol,
			UserId = GetUserId()
		};
	}
}
