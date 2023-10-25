using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Stonks.Administration.Application.IoC;
using Stonks.Administration.Application.Requests;
using Stonks.Auth.Application.Services;
using Stonks.Common.Utils.Response;

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

	private static async Task<IResult> BattleRoyale(
		ISender sender, HttpContext ctx)
	{
		if (!await IsAdmin(ctx))
			return TypedResults.Unauthorized();

		var response = await sender.Send(new BattleRoyaleRound());
		return response.ToHttpResult();
	}

	private static async Task<IResult> UpdatePrices(
		ISender sender, HttpContext ctx)
	{
		if (!await IsAdmin(ctx))
			return TypedResults.Unauthorized();

		var response = await sender.Send(new UpdateAveragePrices());
		return response.ToHttpResult();
	}

	private static async Task<bool> IsAdmin(HttpContext ctx)
	{
		var token = await ctx.GetTokenAsync("access_token");
		(_, var roles) = AuthService.ReadToken(token);
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
