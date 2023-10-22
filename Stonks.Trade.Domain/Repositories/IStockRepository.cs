namespace Stonks.Trade.Domain.Repositories;

public interface IStockRepository
{
	Task<bool> IsBankrupt(Guid stockId);
	Task<Dictionary<Guid, string>> GetTickers(
		CancellationToken cancellationToken = default);
	Task<DateTime?> LastBankruptDate(
		CancellationToken cancellationToken = default);
}
