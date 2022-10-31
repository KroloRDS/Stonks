using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Stonks.Data.Models;

[Index(nameof(Symbol), IsUnique = true)]
public class Stock : HasId
{
    [MaxLength(50)]
    public string Name { get; set; }

    [MaxLength(5)]
    public string Symbol { get; set; }

    [ConcurrencyCheck]
    public int PublicallyOfferredAmount { get; set; }

    public bool Bankrupt { get; set; }
    public DateTime? BankruptDate { get; set; }
}
