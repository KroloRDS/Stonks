using Stonks.Common.Db;
using EF = Stonks.Common.Db.EntityFrameworkModels;
using Stonks.Trade.Domain.Repositories;
using Stonks.Common.Utils.Models.Constants;

namespace Stonks.Trade.Db.Repositories;

public class UserRepository : IUserRepository
{
	private readonly AppDbContext _writeCtx;
	private readonly ReadOnlyDbContext _readCtx;

	public UserRepository(AppDbContext writeCtx,
		ReadOnlyDbContext readCtx)
	{
		_writeCtx = writeCtx;
		_readCtx = readCtx;
	}

	public async Task<decimal> GetBalance(Guid userId,
		CancellationToken cancellationToken = default)
	{
		var user = await _readCtx.GetById<EF.User>(userId) ??
			throw new KeyNotFoundException($"User: {userId}");

		return user.Funds;
	}

	public async Task ChangeBalance(Guid userId, decimal balance,
		CancellationToken cancellationToken = default)
	{
		var user = await _writeCtx.GetById<EF.User>(userId) ??
			throw new KeyNotFoundException($"User: {userId}");

		if (user.Funds + balance < 0) throw new InsufficientFundsException();
		user.Funds += balance;
	}
}
