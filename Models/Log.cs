namespace Stonks.Models;
public class Log
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; }
    public string Details { get; set; }
}