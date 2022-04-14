using Stonks.Managers.Trade;
using Stonks.Managers.Common;
using Stonks.Managers.Bankruptcy;

namespace Stonks.Helpers;

public class ServicesHelper
{
	public static void AddServices(WebApplicationBuilder builder)
	{
		var services = builder.Services;

		services.AddScoped<ILogManager, LogManager>();
		services.AddScoped<IOfferManager, OfferManager>();
		services.AddScoped<IGetPriceManager, GetPriceManager>();
		services.AddScoped<IUserBalanceManager, UserBalanceManager>();
		services.AddScoped<ITransferSharesManager, TransferSharesManager>();
		services.AddScoped<IBankruptSharesManager, BankruptSharesManager>();
	}
}
