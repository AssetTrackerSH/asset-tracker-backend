using PortfolioTracker.Infrastructure.ExternalServices.Binance.Models;

namespace PortfolioTracker.Infrastructure.ExternalServices.Binance;

public interface IBinanceClient
{
    Task<IReadOnlyList<BinanceTickerResponse>> GetTickerPricesAsync(IEnumerable<string> symbols, CancellationToken cancellationToken = default);
}
