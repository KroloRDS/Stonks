using Stonks.Util;
using Stonks.Data;
using Stonks.Data.Models;

namespace Stonks.CQRS.Helpers;

public interface IGiveMoney
{
	Task Handle(Guid userId, decimal amount);
}

public class GiveMoney : IGiveMoney
{
    private readonly AppDbContext _ctx;

    public GiveMoney(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task Handle(Guid userId, decimal amount)
    {
        amount.AssertPositive();
        var user = await _ctx.GetById<User>(userId);
        user.Funds += amount;
    }
}
