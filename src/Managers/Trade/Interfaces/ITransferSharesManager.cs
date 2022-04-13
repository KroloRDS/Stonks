using Stonks.DTOs;

namespace Stonks.Managers.Trade;

public interface ITransferSharesManager
{
	void TransferShares(TransferSharesCommand? command);
}
