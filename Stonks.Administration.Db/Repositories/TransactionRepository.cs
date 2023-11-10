using AutoMapper;
using Stonks.Administration.Domain.Models;
using Stonks.Administration.Domain.Repositories;
using CommonRepositories = Stonks.Common.Db.Repositories;

namespace Stonks.Administration.Db.Repositories;

public class TransactionRepository : ITransactionRepository
{
	private readonly IMapper _mapper;
	private readonly CommonRepositories.ITransactionRepository _transaction;
	

	public TransactionRepository(IMapper mapper,
		CommonRepositories.ITransactionRepository transaction)
	{
		_mapper = mapper;
		_transaction = transaction;
	}

	public IEnumerable<Transaction> Get(
		Guid? stockId = null,
		Guid? userId = null,
		DateTime? fromDate = null)
	{
		var transactions = _transaction.Get(
			stockId, userId, fromDate);
		return transactions.Select(_mapper.Map<Transaction>);
	}
}
