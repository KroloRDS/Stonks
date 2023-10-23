using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Stonks.Administration.Domain.Models;
using Stonks.Administration.Domain.Repositories;
using Stonks.Common.Db;
using EF = Stonks.Common.Db.EntityFrameworkModels;

namespace Stonks.Administration.Db.Repositories;

public class UserRepository : IUserRepository
{
	private readonly IMapper _mapper;
	private readonly AppDbContext _writeCtx;
	private readonly ReadOnlyDbContext _readCtx;

	public UserRepository(IMapper mapper, AppDbContext writeCtx,
		ReadOnlyDbContext readCtx)
	{
		_mapper = mapper;
		_writeCtx = writeCtx;
		_readCtx = readCtx;
	}

	public async Task<bool> LoginExist(string login,
		CancellationToken cancellationToken = default) =>
		await _readCtx.User.AnyAsync(x => x.Login == login, cancellationToken);

	public async Task<User> GetUserFromLogin(string login,
		CancellationToken cancellationToken = default)
	{
		var user = await _readCtx.User.SingleAsync(x => 
			x.Login == login, cancellationToken);
		return _mapper.Map<User>(user);
	}

	public async Task<Guid?> GetUserIdFromToken(Guid token,
		CancellationToken cancellationToken = default)
	{
		var user = await _readCtx.User.SingleOrDefaultAsync(x =>
			x.Token == token && x.TokenExpiration > DateTime.Now,
			cancellationToken);
		return user?.Id;
	}

	public async Task<Guid> RefreshToken(Guid userId, DateTime tokenExpiration,
		CancellationToken cancellationToken = default)
	{
		var token = Guid.NewGuid();
		var user = await _writeCtx.User.SingleAsync(x =>
			x.Id == userId, cancellationToken);

		user.Token = token;
		user.TokenExpiration = tokenExpiration;
		return token;
	}

	public async Task<Guid> Add(string login, short salt, byte[] hash,
		CancellationToken cancellationToken = default)
	{
		var id = Guid.NewGuid();
		await _writeCtx.AddAsync(new EF.User
		{
			Login = login,
			Salt = salt,
			PasswordHash = hash,
			Funds = 0M,
			Id = id,
		}, cancellationToken);
		return id;
	}
}
