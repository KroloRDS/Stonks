using Microsoft.Extensions.DependencyInjection;
using Stonks.Auth.Db;
using Stonks.Auth.Db.AutoMapper;
using Stonks.Auth.Db.Repositories;
using Stonks.Auth.Domain.Repositories;
using System.Reflection;

namespace Stonks.Auth.Application.IoC;

public static class AuthModuleServices
{
	public static IServiceCollection AddAuthModule(
		this IServiceCollection services)
	{
		var assembly = Assembly.GetExecutingAssembly();

		services.AddMediatR(a => a.RegisterServicesFromAssemblies(assembly))
			.AddAutoMapper()
			.AddScoped<IDbWriter, DbWriter>()
			.AddScoped<IUserRepository, UserRepository>();

		return services;
	}
}
