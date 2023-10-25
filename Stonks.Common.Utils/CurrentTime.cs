namespace Stonks.Common.Utils;

public interface ICurrentTime
{
	DateTime Get();
}

public class CurrentTime : ICurrentTime
{
	public DateTime Get() => DateTime.Now;
}