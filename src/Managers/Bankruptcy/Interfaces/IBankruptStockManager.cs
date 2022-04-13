namespace Stonks.Managers.Bankruptcy;
public interface IBankruptStockManager
{
	void Bankrupt(Guid? stockId);
	DateTime? GetLastBankruptDate();
	int GetPublicStocksAmount(Guid? stockId);
	void EmitNewStocks(int amount);
}