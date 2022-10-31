using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stonks.Data.Models;

public class TradeOffer : HasId
{
    [Required]
    public Guid StockId { get; set; }
    public Stock Stock { get; set; }

    public Guid? WriterId { get; set; }
    public User? Writer { get; set; }

    [Required]
    public OfferType Type { get; set; }
    [Required]
    public int Amount { get; set; }

    [Column(TypeName = "decimal(8,2)")]
    public decimal Price { get; set; }
}

public enum OfferType
{
    Buy,
    Sell,
    PublicOfferring
}
