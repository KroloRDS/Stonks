using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace Stonks.Common.Db.EntityFrameworkModels;

[Index(nameof(Symbol), IsUnique = true)]
public class Stock : HasId
{
	public const decimal DEFAULT_PRICE = 1M;

	[MaxLength(50)]
	public string Name { get; set; } = string.Empty;

	[MaxLength(5)]
	public string Symbol { get; set; } = string.Empty;

	public DateTime? BankruptDate { get; set; }
}
