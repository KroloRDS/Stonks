using Stonks.Contracts.Queries;

namespace Stonks.Handlers.Queries;

public interface IQueryHandler<Request, Response> where Request : Query
{
	Response Handle(Request request);
}
