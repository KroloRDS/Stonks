﻿using Stonks.Data.Models;

namespace Stonks.CQRS.Queries.Common;

public record GetTransactionsResponse(IEnumerable<Transaction> Transactions);