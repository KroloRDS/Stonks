namespace Stonks.Contracts.Queries.Common;

public class GetTransactionsQuery : Query
{
	Guid StockId { get; set; }
	DateTime? From { get; set; }

	public override void Validate()
	{
		if (StockId == default)
		{
			throw new ArgumentNullException(nameof(StockId));
		}
	}
}
