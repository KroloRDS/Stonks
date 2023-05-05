using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace Stonks.Data.Models;

[Index(nameof(Timestamp))]
public class Transaction : HasId
{
    [Required]
    public Guid StockId { get; set; }
    [Required]
    public Stock Stock { get; set; }

    [Required]
    public Guid BuyerId { get; set; }
    [Required]
    public User Buyer { get; set; }

    public Guid? SellerId { get; set; }
    public User? Seller { get; set; }

    [Required]
    public int Amount { get; set; }
    [Required]
    public DateTime Timestamp { get; set; }
    [Required]
    [Column(TypeName = "decimal(8,2)")]
    public decimal Price { get; set; }
}
