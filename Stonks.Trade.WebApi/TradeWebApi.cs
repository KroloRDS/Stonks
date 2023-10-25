using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Stonks.Auth.Application.Services;
using Stonks.Common.Utils.Response;
using Stonks.Trade.Application.IoC;
using Stonks.Trade.Application.Requests;

namespace Stonks.Trade.WebApi;

public static class TradeWebApi
{
	public static IServiceCollection AddTradeEndpoints(
		this IServiceCollection services)
	{
		services.AddTradeModule()
			.AddSwaggerGen();

		return services;
	}

	public static IApplicationBuilder UseTradeEndpoints(
		this IApplicationBuilder app)
	{
		app.UseEndpoints(app =>
		{
			app.MapDelete("trade/cancelOffer", CancelOffer);
			app.MapGet("trade/stocks", GetStocks);
			app.MapGet("trade/userOffers", GetUserOffers);
			app.MapPost("trade/placeOffer", PlaceOffer);
		});

		return app;
	}

	private static async Task<IResult> CancelOffer(
		ISender sender, Guid offerId, HttpContext ctx)
	{
		var userId = await GetUserId(ctx);
		if (!userId.HasValue) return TypedResults.Unauthorized();

		var response = await sender.Send(new CancelOffer(userId.Value, offerId));
		return response.ToHttpResult();
	}

	private static async Task<IResult> GetStocks(ISender sender)
	{
		var response = await sender.Send(new GetStocks());
		return response.ToHttpResult();
	}

	private static async Task<IResult> GetUserOffers(
		ISender sender, HttpContext ctx)
	{
		var userId = await GetUserId(ctx);
		if (!userId.HasValue) return TypedResults.Unauthorized();

		var response = await sender.Send(new GetUserOffers(userId.Value));
		return response.ToHttpResult();
	}

	private static async Task<IResult> PlaceOffer(ISender sender,
		[FromBody] PlaceOffer request, HttpContext ctx)
	{
		var userId = await GetUserId(ctx);
		if (!userId.HasValue) return TypedResults.Unauthorized();

		request.WriterId = userId.Value;
		var response = await sender.Send(request);
		return response.ToHttpResult();
	}

	private static async Task<Guid?> GetUserId(HttpContext ctx)
	{
		var token = await ctx.GetTokenAsync("access_token");
		(var id, _) = AuthService.ReadToken(token);
		return id;
	}

	public static IResult ToHttpResult<T>(this Response<T> response)
	{
		return response.Kind switch
		{
			Kind.Ok => TypedResults.Ok(response.Value),
			_ => new Response(response.Kind, response.Message).ToHttpResult()
		};
	}

	public static IResult ToHttpResult(this Response response)
	{
		return response.Kind switch
		{
			Kind.Ok => TypedResults.Ok(),
			Kind.BadRequest => TypedResults.BadRequest(response.Message),
			Kind.ServerError => TypedResults.Problem(response.Message),
			_ => TypedResults.Problem("Unknown problem")
		};
	}
}
