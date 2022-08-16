namespace Stonks.Contracts.Commands.Trade;

public record TakeMoneyCommand(Guid UserId, decimal Amount);
