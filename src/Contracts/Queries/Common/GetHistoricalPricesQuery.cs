namespace Stonks.Contracts.Queries.Common;

public class GetHistoricalPricesQuery : Query
{
	public Guid StockId { get; set; }
	public DateTime FromDate { get; set; }
	public DateTime? ToDate { get; set; }

	public override void Validate()
	{
		if (StockId == default)
		{
			throw new ArgumentNullException(nameof(StockId));
		}

		if (FromDate == default)
		{
			throw new ArgumentNullException(nameof(FromDate));
		}
	}
}
