using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using PortfolioTracker.Infrastructure.ExternalServices.TradingView.Models;

namespace PortfolioTracker.Infrastructure.ExternalServices.TradingView;

public class TradingViewClient : ITradingViewClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TradingViewClient> _logger;

    public TradingViewClient(HttpClient httpClient, ILogger<TradingViewClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Verilen semboller için TradingView'dan anlık fiyat çeker.
    // API key gerektirmez; Türkiye borsası için /turkey/scan endpointi kullanılır.
    public async Task<IReadOnlyList<TradingViewScanRow>> GetQuotesAsync(
        IEnumerable<string> symbols,
        CancellationToken cancellationToken = default)
    {
        var symbolList = symbols.ToList();

        if (symbolList.Count == 0)
            return [];

        try
        {
            _logger.LogInformation("Fetching quotes for {Count} symbols from TradingView...", symbolList.Count);

            // POST body oluşturuluyor:
            // - Columns: hangi alanların döneceği ve hangi sırayla
            // - Filter: sadece istenen sembolleri filtreler
            // - Range: kaç satır döneceği (sembol sayısı kadar)
            var request = new TradingViewScanRequest
            {
                Columns = ["name", "description", "close", "change"],
                Filter =
                [
                    new TradingViewFilter
                    {
                        Left = "name",
                        Operation = "in_range", // "name IN (THYAO, AKBNK, ...)" filtresi
                        Right = symbolList
                    }
                ],
                Range = [0, symbolList.Count]
            };

            // TradingView Scanner API GET değil POST kabul eder
            var response = await _httpClient.PostAsJsonAsync("/turkey/scan", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var scanResponse = await response.Content.ReadFromJsonAsync<TradingViewScanResponse>(cancellationToken: cancellationToken);

            if (scanResponse is null)
            {
                _logger.LogWarning("TradingView returned null response");
                return [];
            }

            _logger.LogDebug("Fetched {Count} quotes from TradingView", scanResponse.Data.Count);

            return scanResponse.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching quotes from TradingView for symbols: {Symbols}", string.Join(", ", symbolList));
            throw;
        }
    }
}
