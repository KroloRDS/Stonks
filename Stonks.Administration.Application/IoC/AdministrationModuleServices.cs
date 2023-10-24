using Microsoft.Extensions.DependencyInjection;
using Stonks.Administration.Application.Services;
using Stonks.Administration.Db;
using Stonks.Administration.Db.Repositories;
using Stonks.Administration.Domain.Repositories;
using System.Reflection;

namespace Stonks.Administration.Application.IoC;

public static class AdministrationModuleServices
{
	public static IServiceCollection AddAdministrationModule(
		this IServiceCollection services)
	{
		var assembly = Assembly.GetExecutingAssembly();

		services.AddMediatR(a => a.RegisterServicesFromAssemblies(assembly))
			.AddScoped<IAuthService, AuthService>()
			.AddScoped<IStockEvaluator, StockEvaluator>()
			.AddScoped<IDbWriter, DbWriter>()
			.AddScoped<IOfferRepository, OfferRepository>()
			.AddScoped<IPriceRepository, PriceRepository>()
			.AddScoped<IShareRepository, ShareRepository>()
			.AddScoped<IStockRepository, StockRepository>()
			.AddScoped<ITransactionRepository, TransactionRepository>()
			.AddScoped<IUserRepository, UserRepository>();

		return services;
	}
}
