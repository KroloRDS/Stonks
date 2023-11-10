namespace Stonks.Common.Db.EntityFrameworkModels;

public interface IHasId
{
	public Guid Id { get; }
};

public class HasId : IHasId
{
	public Guid Id { get; set; }
}
