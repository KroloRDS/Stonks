using Stonks.Contracts.Commands;

namespace Stonks.Handlers.Commands;

public interface ICommandHandler<Request> where Request : Command
{
	void Handle(Request request);
}
