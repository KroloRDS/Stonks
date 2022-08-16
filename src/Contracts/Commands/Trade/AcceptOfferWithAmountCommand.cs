namespace Stonks.Contracts.Commands.Trade;

public record AcceptOfferWithAmountCommand(
	Guid UserId, Guid OfferId, decimal Amount);
