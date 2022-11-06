using MediatR;
using Stonks.Data;

namespace Stonks.CQRS.Commands;

public abstract class BaseCommand<Request> : BaseRequest<Request, Unit>
	where Request : IRequest
{
	protected readonly AppDbContext _ctx;

	protected BaseCommand(AppDbContext ctx,
		IMediator? mediator = null) : base(mediator)
	{
		_ctx = ctx;
	}
}
