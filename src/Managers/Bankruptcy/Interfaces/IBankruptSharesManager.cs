namespace Stonks.Managers.Bankruptcy;

public interface IBankruptSharesManager
{
	void RemoveAllShares(Guid? stockId);
	int GetTotalAmountOfShares(Guid? stockId);
}
