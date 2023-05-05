using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stonks.Data.Models;

[Index(nameof(Symbol), IsUnique = true)]
public class Stock : HasId
{
	public const decimal DEFAULT_PRICE = 1M;

	[MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(5)]
    public string Symbol { get; set; } = string.Empty;

	public int PublicallyOfferredAmount { get; set; }

    public bool Bankrupt { get; set; } = false;
    public DateTime? BankruptDate { get; set; }

	public ulong SharesTraded { get; set; } = 0;

	[Column(TypeName = "decimal(8,2)")]
	public decimal Price { get; set; } = DEFAULT_PRICE;
	public DateTime? LastPriceUpdate { get; set; }
}
