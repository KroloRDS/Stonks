namespace Stonks.Contracts.Commands.Trade;

public record TransferMoneyCommand(
	Guid PayerId, Guid RecipientId, decimal Amount);
