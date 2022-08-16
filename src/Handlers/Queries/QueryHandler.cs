using Stonks.Contracts.Queries;

namespace Stonks.Handlers.Queries;

public abstract class QueryHandler<Request, Response>
	: IQueryHandler<Request, Response> where Request : Query
{
	public Response Handle(Request query)
	{
		if (query is null) throw new ArgumentNullException(nameof(query));
		Validate(query);
		return AfterValidate(query);
	}

	public void Validate(Request query)
	{
		query.Validate();
	}

	public abstract Response AfterValidate(Request query);
}
