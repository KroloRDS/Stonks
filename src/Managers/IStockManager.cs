namespace Stonks.Managers;
public interface IStockManager
{
	void Bankrupt(Guid? stockId);
	DateTime? GetLastBankruptDate();
	int GetPublicStocksAmount(Guid? stockId);
	void EmitNewStocks(int amount);
}