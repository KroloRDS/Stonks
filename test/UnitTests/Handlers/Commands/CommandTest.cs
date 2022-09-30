using System.Threading;
using MediatR;
using Stonks.Models;

namespace UnitTests.Handlers.Commands;

public abstract class CommandTest<Request> : HandlerTest<Request, Unit>
	where Request : IRequest
{
	protected void Handle(Request request)
	{
		_handler.Handle(request, CancellationToken.None).Wait();
		_ctx.SaveChanges();
	}
}
