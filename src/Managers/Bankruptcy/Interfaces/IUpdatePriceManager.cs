namespace Stonks.Managers.Bankruptcy;

public interface IUpdatePriceManager
{
	public const decimal DEFAULT_PRICE = 1M;

	void UpdateAveragePrices();
	void UpdateAveragePriceForOneStock(Guid? stockId);
}
