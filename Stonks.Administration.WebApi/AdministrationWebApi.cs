using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Stonks.Administration.Application.IoC;
using Stonks.Administration.Application.Requests;
using Stonks.Common.Utils.Models;
using Stonks.Common.Utils.Services;

namespace Stonks.Administration.WebApi;

public static class AdministrationEndpoints
{
	public static IServiceCollection AddAdministrationEndpoints(
		this IServiceCollection services)
	{
		services.AddAdministrationModule()
			.AddSwaggerGen();

		return services;
	}

	public static IApplicationBuilder UseAdministrationEndpoints(
		this IApplicationBuilder app)
	{
		app.UseEndpoints(app =>
		{
			app.MapGet("admin/battleRoyale", BattleRoyale);
			app.MapGet("admin/updatePrices", UpdatePrices);
		});

		return app;
	}

	private static async Task<IResult> BattleRoyale(ISender sender,
		IAuthService auth, HttpContext ctx, string? token)
	{
		if (!await IsAdmin(ctx, auth, token))
			return TypedResults.Unauthorized();

		var response = await sender.Send(new BattleRoyaleRound());
		return response.ToHttpResult();
	}

	private static async Task<IResult> UpdatePrices(ISender sender,
		IAuthService auth, HttpContext ctx, string? token)
	{
		if (!await IsAdmin(ctx, auth, token))
			return TypedResults.Unauthorized();

		var response = await sender.Send(new UpdateAveragePrices());
		return response.ToHttpResult();
	}

	private static async Task<bool> IsAdmin(HttpContext ctx,
		IAuthService auth, string? tokenParam = null)
	{
		var token = await ctx.GetTokenAsync("access_token") ?? tokenParam;
		(_, var roles) = auth.ReadToken(token);
		return roles.Contains("Admin");
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
