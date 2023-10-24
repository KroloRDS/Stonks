using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Stonks.Administration.Application.IoC;
using Stonks.Administration.Application.Requests;
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
			app.MapPost("auth/login", Login);
			app.MapPost("auth/register", Register);
			app.MapGet("admin/updatePrices", UpdatePrices);
		});

		return app;
	}

	private static async Task<IResult> BattleRoyale(
		ISender sender, Guid? token)
	{
		var response = await sender.Send(new BattleRoyaleRound());
		return response.ToHttpResult();
	}

	private static async Task<IResult> Login(ISender sender,
		[FromBody] Login request)
	{
		var response = await sender.Send(request);
		return response.ToHttpResult();
	}

	private static async Task<IResult> Register(ISender sender,
		[FromBody] Register request)
	{
		var response = await sender.Send(request);
		return response.ToHttpResult();
	}

	private static async Task<IResult> UpdatePrices(
		ISender sender, Guid? token)
	{
		var response = await sender.Send(new UpdateAveragePrices());
		return response.ToHttpResult();
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
