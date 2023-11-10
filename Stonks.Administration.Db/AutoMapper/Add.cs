using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Stonks.Administration.Db.AutoMapper;

public static class Add
{
	public static IServiceCollection AddAutoMapper(
		this IServiceCollection services)
	{
		services.AddAutoMapper(Assembly.GetExecutingAssembly());
		return services;
	}
}
