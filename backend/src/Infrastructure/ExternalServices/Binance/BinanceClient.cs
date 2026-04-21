using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PortfolioTracker.Infrastructure.ExternalServices.Binance.Models;

namespace PortfolioTracker.Infrastructure.ExternalServices.Binance;

public class BinanceClient : IBinanceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BinanceClient> _logger;

    public BinanceClient(HttpClient httpClient, ILogger<BinanceClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<BinanceTickerResponse>> GetTickerPricesAsync(
        IEnumerable<string> symbols,
        CancellationToken cancellationToken = default)
    {
        var symbolList = symbols.ToList();

        try
        {
            _logger.LogInformation("Fetching prices for {Count} symbols from Binance...", symbolList.Count);

            // Binance batch endpoint: /api/v3/ticker/price?symbols=["BTCUSDT","ETHUSDT"]
            var symbolsJson = JsonSerializer.Serialize(symbolList);
            var url = $"/api/v3/ticker/price?symbols={Uri.EscapeDataString(symbolsJson)}";

            var result = await _httpClient.GetFromJsonAsync<List<BinanceTickerResponse>>(url, cancellationToken);

            if (result is null)
                throw new InvalidOperationException("Failed to deserialize Binance response.");

            _logger.LogDebug("Fetched {Count} crypto prices from Binance", result.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching prices from Binance for symbols: {Symbols}", string.Join(", ", symbolList));
            throw;
        }
    }
}
