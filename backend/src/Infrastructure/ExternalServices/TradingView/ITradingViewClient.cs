using PortfolioTracker.Infrastructure.ExternalServices.TradingView.Models;

namespace PortfolioTracker.Infrastructure.ExternalServices.TradingView;

public interface ITradingViewClient
{
    Task<IReadOnlyList<TradingViewScanRow>> GetQuotesAsync(IEnumerable<string> symbols, CancellationToken cancellationToken = default);
}
