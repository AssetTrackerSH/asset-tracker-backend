using PortfolioTracker.Infrastructure.ExternalServices.GoldApi.Models;

namespace PortfolioTracker.Infrastructure.ExternalServices.GoldApi;

public interface IGoldApiClient
{
    Task<GoldApiResponse> GetSpotPriceAsync(string symbol, CancellationToken cancellationToken = default);
}
