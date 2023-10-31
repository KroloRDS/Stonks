using Microsoft.Extensions.DependencyInjection;
using Stonks.Trade.Application.Services;
using Stonks.Trade.Db;
using Stonks.Trade.Db.Repositories;
using Stonks.Trade.Domain.Repositories;
using System.Reflection;

namespace Stonks.Trade.Application.IoC;

public static class TradeModuleServices
{
	public static IServiceCollection AddTradeModule(
		this IServiceCollection services)
	{
		var assembly = Assembly.GetExecutingAssembly();

		services.AddMediatR(a => a.RegisterServicesFromAssemblies(assembly))
			.AddScoped<IOfferService, OfferService>()
			.AddScoped<IShareService, ShareService>()
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
