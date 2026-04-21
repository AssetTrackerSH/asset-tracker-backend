using PortfolioTracker.Infrastructure.ExternalServices.Twelvedata.Models;

namespace PortfolioTracker.Infrastructure.ExternalServices.Twelvedata;

public interface ITwelvedataClient
{
    Task<IReadOnlyList<TwelvedataQuoteResponse>> GetQuotesAsync(IEnumerable<string> symbols, CancellationToken cancellationToken = default);
}
