using MediatR;
using System.Threading;

namespace UnitTests.CQRS.Commands;

public abstract class CommandTest<Request> : CQRSTest<Request, Unit>
	where Request : IRequest
{
	protected void Handle(Request request)
	{
		_handler.Handle(request, CancellationToken.None).Wait();
		_ctx.SaveChanges();
	}
}
