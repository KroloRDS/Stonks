namespace Stonks.Contracts.Commands.Trade;

public record GiveMoneyCommand(Guid UserId, decimal Amount);
