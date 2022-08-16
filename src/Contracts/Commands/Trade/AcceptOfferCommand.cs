namespace Stonks.Contracts.Commands.Trade;

public record AcceptOfferCommand(Guid UserId, Guid OfferId);
