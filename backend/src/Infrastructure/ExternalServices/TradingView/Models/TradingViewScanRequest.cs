using System.Text.Json.Serialization;

namespace PortfolioTracker.Infrastructure.ExternalServices.TradingView.Models;

// TradingView Scanner API'ye gönderilen POST body modeli.
// Endpoint: POST /turkey/scan
public class TradingViewScanRequest
{
    // Hangi alanların döneceğini belirtir — sıra önemli, response bu sırayla gelir.
    // Örn: ["name", "description", "close", "change"] → d[0]=sembol, d[1]=şirket adı, d[2]=fiyat, d[3]=değişim
    [JsonPropertyName("columns")]
    public List<string> Columns { get; set; } = [];

    // Filtreleme kuralları — hangi sembollerin döneceğini belirler
    [JsonPropertyName("filter")]
    public List<TradingViewFilter>? Filter { get; set; }

    // Sayfalama: [başlangıç_index, bitiş_index]
    // Örn: [0, 3] → ilk 3 satırı getir
    [JsonPropertyName("range")]
    public int[] Range { get; set; } = [0, 100];
}

// Tek bir filtre kuralını temsil eder.
// Örn: { left: "name", operation: "in_range", right: ["THYAO","AKBNK"] }
// → "name değeri THYAO veya AKBNK olanları getir" anlamına gelir
public class TradingViewFilter
{
    // Filtrelenecek kolon adı (örn. "name", "close")
    [JsonPropertyName("left")]
    public string Left { get; set; } = string.Empty;

    // Filtre operasyonu: "in_range" = listede olanlar, "greater" = büyük, vb.
    [JsonPropertyName("operation")]
    public string Operation { get; set; } = string.Empty;

    // Filtre değeri — string, sayı veya liste olabilir
    [JsonPropertyName("right")]
    public object Right { get; set; } = new();
}

