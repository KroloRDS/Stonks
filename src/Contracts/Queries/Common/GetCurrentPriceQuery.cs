namespace Stonks.Contracts.Queries.Common;

public class GetCurrentPriceQuery : Query
{
	public Guid StockId { get; set; }

	public override void Validate()
	{
		if (StockId == default)
		{
			throw new ArgumentNullException(nameof(StockId));
		}
	}
}
