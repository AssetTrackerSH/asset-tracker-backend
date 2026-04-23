namespace PortfolioTracker.Infrastructure.Configuration;

// appsettings.json'daki "TradingView" bloğunu bu sınıfa bağlar.
// IOptions<TradingViewOptions> ile inject edilir.
public class TradingViewOptions
{
    // appsettings.json'daki section adıyla eşleşmeli
    public const string SectionName = "TradingView";

    // TradingView Scanner API'nin base URL'i
    public string BaseUrl { get; set; } = "https://scanner.tradingview.com";

    // HTTP isteği için timeout süresi (saniye)
    public int TimeoutSeconds { get; set; } = 10;

    // Fiyatların memory cache'de kaç dakika tutulacağı
    public int CacheExpirationMinutes { get; set; } = 15;

    // Takip edilecek BIST sembol listesi — sadece sembol adı yeterli (borsa bilgisi URL'de implicit)
    public List<string> Symbols { get; set; } = [];
}
