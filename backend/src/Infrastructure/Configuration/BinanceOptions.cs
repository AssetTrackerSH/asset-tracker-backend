namespace PortfolioTracker.Infrastructure.Configuration;

public class BinanceOptions
{
    public const string SectionName = "Binance";

    public string BaseUrl { get; set; } = "https://api.binance.com";
    public int TimeoutSeconds { get; set; } = 30;
    public int CacheExpirationMinutes { get; set; } = 10;
}
