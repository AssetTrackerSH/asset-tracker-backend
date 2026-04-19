using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortfolioTracker.Domain.Entities;
using PortfolioTracker.Domain.Services;
using PortfolioTracker.Domain.ValueObjects;
using PortfolioTracker.Infrastructure.Configuration;
using PortfolioTracker.Infrastructure.ExternalServices.Tcmb;

namespace PortfolioTracker.Infrastructure.Services;

public class CachedExchangeRateProvider : IExchangeRateProvider
{
    private readonly ITcmbClient _tcmbClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedExchangeRateProvider> _logger;
    private readonly TcmbOptions _options;

    private const string ExchangeRatesCacheKeyPrefix = "ExchangeRates";
    private const string PreciousMetalsCacheKeyPrefix = "PreciousMetals";

    public CachedExchangeRateProvider(
        ITcmbClient tcmbClient,
        IMemoryCache cache,
        ILogger<CachedExchangeRateProvider> logger,
        IOptions<TcmbOptions> options)
    {
        _tcmbClient = tcmbClient ?? throw new ArgumentNullException(nameof(tcmbClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<IReadOnlyList<ExchangeRate>> GetLatestExchangeRatesAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ExchangeRatesCacheKeyPrefix}_{DateTime.UtcNow:yyyyMMdd}";

        if (_cache.TryGetValue<IReadOnlyList<ExchangeRate>>(cacheKey, out var cachedRates))
        {
            _logger.LogDebug("Exchange rates found in cache");
            return cachedRates!;
        }

        _logger.LogDebug("Exchange rates not in cache, fetching from TCMB...");

        var response = await _tcmbClient.GetDailyCurrencyRatesAsync(cancellationToken);

        var exchangeRates = response.Currencies
            .Where(c => c.ForexBuying > 0 && c.ForexSelling > 0) // Filter out invalid rates
            .Select(c =>
            {
                try
                {
                    var currencyCode = CurrencyCode.From(c.Code);
                    var date = ParseTcmbDate(response.Date);

                    return new ExchangeRate(
                        currencyCode,
                        c.ForexBuying,
                        c.ForexSelling,
                        c.Unit,
                        date);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse currency {CurrencyCode}, skipping", c.Code);
                    return null;
                }
            })
            .Where(r => r is not null)
            .Cast<ExchangeRate>()
            .ToList();

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheExpirationMinutes)
        };

        _cache.Set(cacheKey, exchangeRates, cacheOptions);

        _logger.LogInformation("Cached {Count} exchange rates for {Duration} minutes",
            exchangeRates.Count, _options.CacheExpirationMinutes);

        return exchangeRates;
    }

    public Task<IReadOnlyList<PreciousMetalPrice>> GetLatestPreciousMetalPricesAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement when TCMB precious metals endpoint is available
        // For now, return empty list
        _logger.LogDebug("Precious metal prices not yet implemented, returning empty list");
        return Task.FromResult<IReadOnlyList<PreciousMetalPrice>>(Array.Empty<PreciousMetalPrice>());
    }

    private static DateTime ParseTcmbDate(string dateString)
    {
        // TCMB date format: "18.04.2026" (dd.MM.yyyy)
        if (DateTime.TryParseExact(
            dateString,
            "dd.MM.yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date))
        {
            return date;
        }

        // Fallback to today if parsing fails
        return DateTime.UtcNow;
    }
}
