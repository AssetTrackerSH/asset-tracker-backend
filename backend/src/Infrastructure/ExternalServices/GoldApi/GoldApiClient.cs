using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using PortfolioTracker.Infrastructure.ExternalServices.GoldApi.Models;

namespace PortfolioTracker.Infrastructure.ExternalServices.GoldApi;

public class GoldApiClient : IGoldApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoldApiClient> _logger;

    public GoldApiClient(HttpClient httpClient, ILogger<GoldApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GoldApiResponse> GetSpotPriceAsync(string symbol, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching spot price for {Symbol} from GoldApi...", symbol);

            var result = await _httpClient.GetFromJsonAsync<GoldApiResponse>(
                $"/price/{symbol}", cancellationToken);

            if (result is null)
            {
                throw new InvalidOperationException($"Failed to deserialize GoldApi response for {symbol}");
            }

            _logger.LogDebug("Fetched {Symbol} = {Price} {Currency}", result.Symbol, result.Price, result.Currency);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching {Symbol} from GoldApi", symbol);
            throw;
        }
    }
}
