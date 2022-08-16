using Stonks.Models;

namespace Stonks.Contracts.Queries.Common;

public record GetTransactionsResponse(IEnumerable<Transaction> Transactions);
