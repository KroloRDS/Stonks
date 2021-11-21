using Stonks.Models;

namespace Stonks.DTOs;

public class PlaceOfferCommand
{
	public Guid? StockId { get; set; }
	public Guid? WriterId { get; set; }
	public int? Amount { get; set; }
	public OfferType? Type { get; set; }
	public decimal? Price { get; set; }
}
