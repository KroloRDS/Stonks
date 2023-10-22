namespace Stonks.Common.Utils;

public interface ILogProvider
{
	bool Log(string className, string message,
		string? exception, string? objectDump);
}
