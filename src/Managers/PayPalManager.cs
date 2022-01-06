namespace Stonks.Managers;

public class PayPalManager : IPayPalManager
{
	private readonly Dictionary<string, string> _payPalConfig;

	public PayPalManager(IConfigurationManager config)
	{
		_payPalConfig = config.PayPalConfig();
	}
}
