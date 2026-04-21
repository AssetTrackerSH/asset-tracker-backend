using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortfolioTracker.Domain.Entities;
using PortfolioTracker.Domain.Services;
using PortfolioTracker.Domain.ValueObjects;
using PortfolioTracker.Infrastructure.Configuration;
using PortfolioTracker.Infrastructure.ExternalServices.Binance;
using PortfolioTracker.Infrastructure.ExternalServices.GoldApi;
using PortfolioTracker.Infrastructure.ExternalServices.Tcmb;

namespace PortfolioTracker.Infrastructure.Services;

public class CachedExchangeRateProvider : IExchangeRateProvider
{
    private readonly ITcmbClient _tcmbClient;
    private readonly IGoldApiClient _goldApiClient;
    private readonly IBinanceClient _binanceClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedExchangeRateProvider> _logger;
    private readonly TcmbOptions _options;
    private readonly BinanceOptions _binanceOptions;

    private const string ExchangeRatesCacheKeyPrefix = "ExchangeRates";
    private const string PreciousMetalsCacheKeyPrefix = "PreciousMetals";
    private const string CryptoCacheKeyPrefix = "Crypto";
    private const decimal TroyOunceToGrams = 31.1034768m;

    private static readonly (string Symbol, PreciousMetalType MetalType)[] SupportedMetals =
    [
        ("XAU", PreciousMetalType.Gold),
        ("XAG", PreciousMetalType.Silver),
        ("XPT", PreciousMetalType.Platinum),
        ("XPD", PreciousMetalType.Palladium),
    ];

    private static readonly string[] SupportedCryptoSymbols =
    [
        "BTCUSDT", "ETHUSDT", "BNBUSDT", "SOLUSDT", "XRPUSDT",
        "ADAUSDT", "DOGEUSDT", "AVAXUSDT", "DOTUSDT", "MATICUSDT"
    ];

    public CachedExchangeRateProvider(
        ITcmbClient tcmbClient,
        IGoldApiClient goldApiClient,
        IBinanceClient binanceClient,
        IMemoryCache cache,
        ILogger<CachedExchangeRateProvider> logger,
        IOptions<TcmbOptions> options,
        IOptions<BinanceOptions> binanceOptions)
    {
        _tcmbClient = tcmbClient ?? throw new ArgumentNullException(nameof(tcmbClient));
        _goldApiClient = goldApiClient ?? throw new ArgumentNullException(nameof(goldApiClient));
        _binanceClient = binanceClient ?? throw new ArgumentNullException(nameof(binanceClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _binanceOptions = binanceOptions?.Value ?? throw new ArgumentNullException(nameof(binanceOptions));
    }

    public async Task<IReadOnlyList<ExchangeRate>> GetLatestExchangeRatesAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = ExchangeRatesCacheKeyPrefix;

        if (_cache.TryGetValue<IReadOnlyList<ExchangeRate>>(cacheKey, out var cachedRates))
        {
            _logger.LogDebug("Exchange rates found in cache");
            return cachedRates!;
        }

        _logger.LogDebug("Exchange rates not in cache, fetching from TCMB...");

        var response = await _tcmbClient.GetDailyCurrencyRatesAsync(cancellationToken);

        var exchangeRates = response.Currencies
            .Where(c => c.ForexBuying > 0 && c.ForexSelling > 0)
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

    public async Task<IReadOnlyList<PreciousMetalPrice>> GetLatestPreciousMetalPricesAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = PreciousMetalsCacheKeyPrefix;

        if (_cache.TryGetValue<IReadOnlyList<PreciousMetalPrice>>(cacheKey, out var cached))
        {
            _logger.LogDebug("Precious metal prices found in cache");
            return cached!;
        }

        var tcmbResponse = await _tcmbClient.GetDailyCurrencyRatesAsync(cancellationToken);
        var usdCurrency = tcmbResponse.Currencies.FirstOrDefault(c => c.Code == "USD");

        if (usdCurrency is null || usdCurrency.ForexBuying <= 0)
        {
            _logger.LogWarning("USD/TRY rate not found in TCMB response, cannot convert precious metal prices");
            return Array.Empty<PreciousMetalPrice>();
        }

        var usdToTryRate = usdCurrency.ForexBuying;
        var date = ParseTcmbDate(tcmbResponse.Date);

        var metals = new List<PreciousMetalPrice>();

        foreach (var (symbol, metalType) in SupportedMetals)
        {
            try
            {
                var spotPrice = await _goldApiClient.GetSpotPriceAsync(symbol, cancellationToken);

                // spotPrice.Price USD/oz → TRY/oz → TRY/gram
                var pricePerOunceInTry = spotPrice.Price * usdToTryRate;
                var pricePerGramInTry = pricePerOunceInTry / TroyOunceToGrams;

                metals.Add(new PreciousMetalPrice(metalType, pricePerGramInTry, pricePerOunceInTry, date));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch price for {Symbol}, skipping", symbol);
            }
        }

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheExpirationMinutes)
        };

        _cache.Set(cacheKey, metals, cacheOptions);

        _logger.LogInformation("Cached {Count} precious metal prices", metals.Count);

        return metals;
    }

    public async Task<IReadOnlyList<CryptoPrice>> GetLatestCryptoPricesAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue<IReadOnlyList<CryptoPrice>>(CryptoCacheKeyPrefix, out var cached))
        {
            _logger.LogDebug("Crypto prices found in cache");
            return cached!;
        }

        // USD/TRY kuru için TCMB'ye git
        var tcmbResponse = await _tcmbClient.GetDailyCurrencyRatesAsync(cancellationToken);
        var usdCurrency = tcmbResponse.Currencies.FirstOrDefault(c => c.Code == "USD");

        if (usdCurrency is null || usdCurrency.ForexBuying <= 0)
        {
            _logger.LogWarning("USD/TRY rate not found in TCMB response, cannot convert crypto prices");
            return Array.Empty<CryptoPrice>();
        }

        var usdToTryRate = usdCurrency.ForexBuying;
        var tickers = await _binanceClient.GetTickerPricesAsync(SupportedCryptoSymbols, cancellationToken);

        var cryptoPrices = tickers
            .Select(t =>
            {
                // "BTCUSDT" → "BTC"
                var symbol = t.Symbol.Replace("USDT", string.Empty, StringComparison.OrdinalIgnoreCase);
                var priceInTry = t.Price * usdToTryRate;
                return new CryptoPrice(symbol, t.Price, priceInTry, DateTime.UtcNow);
            })
            .ToList();

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_binanceOptions.CacheExpirationMinutes)
        };

        _cache.Set(CryptoCacheKeyPrefix, cryptoPrices, cacheOptions);

        _logger.LogInformation("Cached {Count} crypto prices", cryptoPrices.Count);

        return cryptoPrices;
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

        return DateTime.UtcNow;
    }
}
