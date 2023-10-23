namespace Stonks.Trade.Application.DTOs;

public class StockDTO
{
	public string Name { get; set; } = string.Empty;
	public string Ticker { get; set; } = string.Empty;
	public decimal AvgPrice { get; set; }
	public decimal MarketCap { get; set; }
	public double Volatility { get; set; }
	public int PublicallyOfferdShares { get; set; }
}
