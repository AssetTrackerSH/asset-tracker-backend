using PortfolioTracker.Infrastructure.ExternalServices.Tcmb.Models;

namespace PortfolioTracker.Infrastructure.ExternalServices.Tcmb;

public interface ITcmbClient
{
    Task<TcmbCurrencyResponse> GetDailyCurrencyRatesAsync(CancellationToken cancellationToken = default);
}
