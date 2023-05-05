using MediatR;
using Stonks.Data;

namespace Stonks.CQRS.Queries;

public abstract class BaseQuery<Request, Response> :
	BaseRequest<Request, Response> where Request : IRequest<Response>
{
	protected readonly ReadOnlyDbContext _ctx;

	protected BaseQuery(ReadOnlyDbContext ctx,
		IMediator? mediator = null) : base(mediator)
	{
		_ctx = ctx;
	}
}
