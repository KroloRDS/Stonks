namespace Stonks.Contracts.Queries.Bankruptcy;

public class GetPublicStocksAmountQuery : Query
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
