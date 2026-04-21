using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortfolioTracker.Infrastructure.Configuration;
using PortfolioTracker.Infrastructure.ExternalServices.Twelvedata.Models;

namespace PortfolioTracker.Infrastructure.ExternalServices.Twelvedata;

public class TwelvedataClient : ITwelvedataClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TwelvedataClient> _logger;
    private readonly TwelvedataOptions _options;

    public TwelvedataClient(HttpClient httpClient, ILogger<TwelvedataClient> logger, IOptions<TwelvedataOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options.Value;
    }

    public async Task<IReadOnlyList<TwelvedataQuoteResponse>> GetQuotesAsync(
        IEnumerable<string> symbols,
        CancellationToken cancellationToken = default)
    {
        var symbolList = symbols.ToList();

        try
        {
            _logger.LogInformation("Fetching quotes for {Count} symbols from Twelvedata...", symbolList.Count);

            var symbolsParam = string.Join(",", symbolList);
            var url = $"/quote?symbol={symbolsParam}&apikey={_options.ApiKey}";

            var responseBody = await _httpClient.GetStringAsync(url, cancellationToken);

            var results = ParseResponse(responseBody, symbolList);

            _logger.LogDebug("Fetched {Count} stock quotes from Twelvedata", results.Count);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching quotes from Twelvedata for symbols: {Symbols}", string.Join(", ", symbolList));
            throw;
        }
    }

    // Twelvedata tek sembolde düz obje, çok sembolde {"AAPL": {...}, "MSFT": {...}} döner
    private List<TwelvedataQuoteResponse> ParseResponse(string json, List<string> symbols)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (symbols.Count == 1)
        {
            var single = root.Deserialize<TwelvedataQuoteResponse>();
            return single is not null ? [single] : [];
        }

        var results = new List<TwelvedataQuoteResponse>();
        foreach (var symbol in symbols)
        {
            if (root.TryGetProperty(symbol, out var symbolElement))
            {
                var quote = symbolElement.Deserialize<TwelvedataQuoteResponse>();
                if (quote is not null)
                    results.Add(quote);
            }
            else
            {
                _logger.LogWarning("No quote data returned for symbol: {Symbol}", symbol);
            }
        }

        return results;
    }
}
