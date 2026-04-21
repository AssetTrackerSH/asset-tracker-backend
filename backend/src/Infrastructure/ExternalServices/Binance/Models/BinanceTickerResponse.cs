using System.Text.Json.Serialization;

namespace PortfolioTracker.Infrastructure.ExternalServices.Binance.Models;

public class BinanceTickerResponse
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}
