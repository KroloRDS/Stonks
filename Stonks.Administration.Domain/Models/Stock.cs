namespace Stonks.Administration.Domain.Models;

public class Stock
{
	public const decimal DEFAULT_PRICE = 1M;

	public Guid Id { get; set; }
	public DateTime? BankruptDate { get; set; }
}
