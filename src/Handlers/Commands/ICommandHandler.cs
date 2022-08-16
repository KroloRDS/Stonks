namespace Stonks.Handlers.Commands;

public interface ICommandHandler<Command>
{
	void Handle(Command command);
}
