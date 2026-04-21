namespace PortfolioTracker.Infrastructure.Configuration;

public class TwelvedataOptions
{
    public const string SectionName = "Twelvedata";

    public string BaseUrl { get; set; } = "https://api.twelvedata.com";
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 10;
    public int CacheExpirationMinutes { get; set; } = 15;
    public List<StockSymbolConfig> Stocks { get; set; } = [];
}

public class StockSymbolConfig
{
    public string Symbol { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
}
