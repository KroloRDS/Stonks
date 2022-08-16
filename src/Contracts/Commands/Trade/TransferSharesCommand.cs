namespace Stonks.Contracts.Commands.Trade;

public record TransferSharesCommand(
	Guid StockId,
	Guid BuyerId,
	Guid? SellerId,
	int Amount,
	bool BuyFromUser);
