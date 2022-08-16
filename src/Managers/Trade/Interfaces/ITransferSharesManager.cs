using Stonks.Contracts.Commands.Trade;

namespace Stonks.Managers.Trade;

public interface ITransferSharesManager
{
	void TransferShares(TransferSharesCommand? command);
}
