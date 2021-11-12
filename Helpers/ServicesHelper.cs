using Stonks.Managers;

namespace Stonks.Helpers;

public class ServicesHelper
{
	public static void AddServices(WebApplicationBuilder builder)
	{
		var services = builder.Services;

		services.AddScoped<ILogManager, LogManager>();
		services.AddScoped<IStockManager, StockManager>();
	}
}
