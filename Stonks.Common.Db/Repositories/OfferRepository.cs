using Microsoft.EntityFrameworkCore;
using Stonks.Common.Db.EntityFrameworkModels;

namespace Stonks.Common.Db.Repositories;

public interface IOfferRepository
{
	Task<int> PublicallyOfferdAmount(Guid stockId,
		CancellationToken cancellationToken = default);
}

public class OfferRepository : IOfferRepository
{
	private readonly ReadOnlyDbContext _ctx;

	public OfferRepository(ReadOnlyDbContext ctx)
	{
		_ctx = ctx;
	}

	public async Task<int> PublicallyOfferdAmount(Guid stockId,
		CancellationToken cancellationToken = default)
	{
		var offer = await _ctx.TradeOffer.FirstOrDefaultAsync(
			x => x.Type == OfferType.PublicOfferring &&
			x.StockId == stockId, cancellationToken);
		return offer?.Amount ?? 0;
	}
}
