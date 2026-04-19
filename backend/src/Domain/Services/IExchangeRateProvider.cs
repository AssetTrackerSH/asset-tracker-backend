using PortfolioTracker.Domain.Entities;

namespace PortfolioTracker.Domain.Services;

public interface IExchangeRateProvider
{
    Task<IReadOnlyList<ExchangeRate>> GetLatestExchangeRatesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PreciousMetalPrice>> GetLatestPreciousMetalPricesAsync(CancellationToken cancellationToken = default);
}
