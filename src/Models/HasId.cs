namespace Stonks.Models;

public interface IHasId
{
	public Guid Id { get; } 
};

public class HasId : IHasId
{
	public Guid Id { get; set; }
}
