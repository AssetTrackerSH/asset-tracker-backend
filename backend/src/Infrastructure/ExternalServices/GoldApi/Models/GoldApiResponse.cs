using System.Text.Json.Serialization;

namespace PortfolioTracker.Infrastructure.ExternalServices.GoldApi.Models;

public class GoldApiResponse
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
