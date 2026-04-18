namespace PortfolioTracker.Infrastructure.Configuration;

public class TcmbOptions
{
    public const string SectionName = "Tcmb";

    public string BaseUrl { get; set; } = "https://www.tcmb.gov.tr";
    public int CacheExpirationMinutes { get; set; } = 10;
    public int TimeoutSeconds { get; set; } = 30;
}
