namespace Stonks.Contracts.Queries.Bankruptcy;

public class GetTotalAmountOfSharesQuery : Query
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
