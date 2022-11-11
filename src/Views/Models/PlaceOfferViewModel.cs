using Stonks.Data.Models;

namespace Stonks.Views.Models;

public class PlaceOfferViewModel
{
	public Guid UserId { get; set; }
	public Guid StockId { get; set; }
	public string StockSymbol { get; set; } = string.Empty;
	public OfferType OfferType { get; set; }
}
