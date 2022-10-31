using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stonks.Data.Models;

public class AvgPrice : HasId
{
    [Required]
    public Guid StockId { get; set; }
    [Required]
    public Stock Stock { get; set; }
    [Required]
    public DateTime DateTime { get; set; }

    public bool IsCurrent { get; set; }
    public ulong SharesTraded { get; set; }

    [Column(TypeName = "decimal(8,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(8,2)")]
    public decimal PriceNormalised { get; set; }
}
