using Stonks.Models;

namespace Stonks.Contracts.Commands.Trade;

public record PlaceOfferCommand(
	Guid StockId,
	Guid WriterId,
	int Amount,
	OfferType Type,
	decimal Price
);
