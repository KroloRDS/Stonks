using PayPal.Api;

namespace Stonks.Managers;

public class PayPalManager : IPayPalManager
{
	private readonly APIContext _payPalApi;

	public PayPalManager(IConfigurationManager config)
	{
		_payPalApi = InitPaypalApi(config);
	}

	private static APIContext InitPaypalApi(IConfigurationManager config)
	{
		var payPalConfig = config.PayPalConfig();
		var token = new OAuthTokenCredential(payPalConfig).GetAccessToken();
		return new APIContext(token);
	}
}
