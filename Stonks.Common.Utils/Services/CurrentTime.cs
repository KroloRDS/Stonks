namespace Stonks.Common.Utils.Services;

public interface ICurrentTime
{
    DateTime Get();
}

public class CurrentTime : ICurrentTime
{
    public DateTime Get() => DateTime.Now;
}