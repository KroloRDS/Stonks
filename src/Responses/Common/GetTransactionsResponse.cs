using Stonks.Models;

namespace Stonks.Responses.Common;

public record GetTransactionsResponse(IEnumerable<Transaction> Transactions);
