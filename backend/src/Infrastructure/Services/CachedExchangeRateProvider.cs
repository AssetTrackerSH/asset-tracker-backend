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
using PortfolioTracker.Infrastructure.ExternalServices.TradingView;

namespace PortfolioTracker.Infrastructure.Services;

public class CachedExchangeRateProvider : IExchangeRateProvider
{
    private readonly ITcmbClient _tcmbClient;
    private readonly IGoldApiClient _goldApiClient;
    private readonly IBinanceClient _binanceClient;
    private readonly ITradingViewClient _tradingViewClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedExchangeRateProvider> _logger;
    private readonly TcmbOptions _options;
    private readonly BinanceOptions _binanceOptions;
    private readonly TradingViewOptions _tradingViewOptions;

    private const string ExchangeRatesCacheKeyPrefix = "ExchangeRates";
    private const string PreciousMetalsCacheKeyPrefix = "PreciousMetals";
    private const string CryptoCacheKeyPrefix = "Crypto";
    private const string StocksCacheKeyPrefix = "Stocks";

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
        ITradingViewClient tradingViewClient,
        IMemoryCache cache,
        ILogger<CachedExchangeRateProvider> logger,
        IOptions<TcmbOptions> options,
        IOptions<BinanceOptions> binanceOptions,
        IOptions<TradingViewOptions> tradingViewOptions)
    {
        _tcmbClient = tcmbClient ?? throw new ArgumentNullException(nameof(tcmbClient));
        _goldApiClient = goldApiClient ?? throw new ArgumentNullException(nameof(goldApiClient));
        _binanceClient = binanceClient ?? throw new ArgumentNullException(nameof(binanceClient));
        _tradingViewClient = tradingViewClient ?? throw new ArgumentNullException(nameof(tradingViewClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _binanceOptions = binanceOptions?.Value ?? throw new ArgumentNullException(nameof(binanceOptions));
        _tradingViewOptions = tradingViewOptions?.Value ?? throw new ArgumentNullException(nameof(tradingViewOptions));
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

        _cache.Set(cacheKey, exchangeRates, CreateCacheOptions(_options.CacheExpirationMinutes));

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

        var tcmbData = await GetTcmbUsdRateAsync(cancellationToken);
        if (tcmbData is null)
            return Array.Empty<PreciousMetalPrice>();

        var (usdToTryRate, date) = tcmbData.Value;

        var metalResults = await Task.WhenAll(SupportedMetals.Select(async m =>
        {
            try
            {
                var spotPrice = await _goldApiClient.GetSpotPriceAsync(m.Symbol, cancellationToken);
                // spotPrice.Price is USD/oz — convert to TRY/oz then TRY/gram
                var pricePerOunceInTry = spotPrice.Price * usdToTryRate;
                var pricePerGramInTry = pricePerOunceInTry / TroyOunceToGrams;
                return new PreciousMetalPrice(m.MetalType, pricePerGramInTry, pricePerOunceInTry, date);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch price for {Symbol}, skipping", m.Symbol);
                return null;
            }
        }));

        var metals = metalResults.Where(m => m is not null).Cast<PreciousMetalPrice>().ToList();

        _cache.Set(cacheKey, metals, CreateCacheOptions(_options.CacheExpirationMinutes));

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

        var tcmbData = await GetTcmbUsdRateAsync(cancellationToken);
        if (tcmbData is null)
            return Array.Empty<CryptoPrice>();

        var usdToTryRate = tcmbData.Value.UsdToTryRate;
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

        _cache.Set(CryptoCacheKeyPrefix, cryptoPrices, CreateCacheOptions(_binanceOptions.CacheExpirationMinutes));

        _logger.LogInformation("Cached {Count} crypto prices", cryptoPrices.Count);

        return cryptoPrices;
    }

    public async Task<IReadOnlyList<StockPrice>> GetLatestStockPricesAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue<IReadOnlyList<StockPrice>>(StocksCacheKeyPrefix, out var cached))
        {
            _logger.LogDebug("Stock prices found in cache");
            return cached!;
        }

        var tcmbData = await GetTcmbUsdRateAsync(cancellationToken);
        if (tcmbData is null)
            return Array.Empty<StockPrice>();

        var usdToTryRate = tcmbData.Value.UsdToTryRate;
        var symbols = _tradingViewOptions.Symbols;

        if (symbols.Count == 0)
        {
            _logger.LogWarning("No stock symbols configured in TradingViewOptions");
            return Array.Empty<StockPrice>();
        }

        // TradingView'a POST isteği atılır; BIST fiyatları TRY cinsinden döner
        var quotes = await _tradingViewClient.GetQuotesAsync(symbols, cancellationToken);

        var stockPrices = new List<StockPrice>();

        foreach (var quote in quotes)
        {
            try
            {
                // Piyasa kapalıysa veya veri yoksa Close null gelebilir
                if (quote.Close is null)
                {
                    _logger.LogWarning("No price returned for symbol: {Symbol}", quote.Symbol);
                    continue;
                }

                // TradingView BIST fiyatını direkt TRY olarak verir — USD→TRY çevirisine gerek yok.
                // USD değeri ise tersine hesaplanır.
                var priceInTry = quote.Close.Value;
                var priceInUsd = usdToTryRate > 0 ? priceInTry / usdToTryRate : 0;

                stockPrices.Add(new StockPrice(quote.Symbol, quote.Description, "BIST", priceInUsd, priceInTry, DateTime.UtcNow));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process stock quote for {Symbol}, skipping", quote.Symbol);
            }
        }

        _cache.Set(StocksCacheKeyPrefix, stockPrices, CreateCacheOptions(_tradingViewOptions.CacheExpirationMinutes));

        _logger.LogInformation("Cached {Count} stock prices", stockPrices.Count);

        return stockPrices;
    }

    private async Task<(decimal UsdToTryRate, DateTime Date)?> GetTcmbUsdRateAsync(CancellationToken cancellationToken)
    {
        var rates = await GetLatestExchangeRatesAsync(cancellationToken);
        var usd = rates.FirstOrDefault(r => r.CurrencyCode.Code == "USD");

        if (usd is null)
        {
            _logger.LogWarning("USD/TRY rate not found in TCMB response");
            return null;
        }

        return (usd.GetEffectiveBuyingRate(), usd.Date);
    }

    private static MemoryCacheEntryOptions CreateCacheOptions(int expirationMinutes) =>
        new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes) };

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
