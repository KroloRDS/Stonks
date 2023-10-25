using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Stonks.Auth.Application.IoC;
using Stonks.Auth.Application.Requests;
using Stonks.Common.Utils.Configuration;
using Stonks.Common.Utils.Response;
using System.Text;

namespace Stonks.Auth.WebApi;

public static class AuthEndpoints
{
	public static IServiceCollection AddAuthEndpoints(
		this IServiceCollection services, JwtConfiguration jwtConfiguration)
	{
		services.AddAuthModule()
			.AddSwaggerGen()
			.AddJwtAuth(jwtConfiguration);

		return services;
	}

	private static IServiceCollection AddJwtAuth(
		this IServiceCollection services, JwtConfiguration jwtConfiguration)
	{
		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(opts =>
			{
				byte[] signingKeyBytes = Encoding.UTF8
					.GetBytes(jwtConfiguration.SigningKey);

				opts.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = jwtConfiguration.Issuer,
					ValidAudience = jwtConfiguration.Audience,
					IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes)
				};
			});

		return services;
	}

	public static IApplicationBuilder UseAuthEndpoints(
		this IApplicationBuilder app)
	{
		app.UseEndpoints(app =>
		{
			app.MapPost("auth/login", Login);
			app.MapPost("auth/register", Register);
		});

		return app;
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
