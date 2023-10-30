namespace Stonks.Common.Utils;

public interface ILogProvider
{
	void Log(string className, string message,
		string? exception, string? objectDump);
}
