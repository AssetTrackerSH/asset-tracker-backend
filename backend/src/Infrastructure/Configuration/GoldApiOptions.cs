namespace PortfolioTracker.Infrastructure.Configuration;

public class GoldApiOptions
{
    public const string SectionName = "GoldApi";

    public string BaseUrl { get; set; } = "https://api.gold-api.com";
    public int TimeoutSeconds { get; set; } = 30;
}
