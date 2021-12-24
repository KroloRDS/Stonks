namespace Stonks.Managers;
public interface IStockManager
{
	void Bankrupt(Guid? stockId);
	DateTime GetLastBankruptDate();
}