namespace Stonks.DTOs;

public class StockIndicator
{
	public Guid StockId { get; set; }
	public decimal MarketCap { get; set; }
	public int StocksAmount { get; set; }
	public double Volatility { get; set; }
	public double Fun { get; set; }
}

public class StockIndicatorNormalised
{
	public Guid StockId { get; set; }
	public double MarketCap{ get; set; }
	public double StocksAmount { get; set; }
	public double Volatility { get; set; }
	public double Fun { get; set; }
}
