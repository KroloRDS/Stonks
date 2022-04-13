namespace Stonks.Managers.Bankruptcy;

public interface IBankruptSharesManager
{
	void RemoveAllShares(Guid? stockId);
	int GetAllSharesAmount(Guid? stockId);
}
