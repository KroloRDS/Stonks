using MediatR;
using Stonks.Util;

namespace Stonks.CQRS;

public abstract class BaseRequest<Request, Response> :
	IRequestHandler<Request, Response> where Request : IRequest<Response>
{
	protected readonly IMediator _mediator;

	protected BaseRequest(IMediator? mediator = null)
	{
		_mediator = mediator ?? new FakeMediator();
	}

	public abstract Task<Response> Handle(Request request,
		CancellationToken cancellationToken);
}
