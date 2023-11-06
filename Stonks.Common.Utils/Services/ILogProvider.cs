namespace Stonks.Common.Utils.Services;

public interface ILogProvider
{
    void Log(string className, string message,
        string? exception, string? objectDump);
}
