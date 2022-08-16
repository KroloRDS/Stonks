using Stonks.Requests.Commands.Trade;

namespace Stonks.Managers.Trade;

public interface ITransferSharesManager
{
	void TransferShares(TransferSharesCommand? command);
}
