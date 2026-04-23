using System.Text.Json.Serialization;

namespace PortfolioTracker.Infrastructure.ExternalServices.TradingView.Models;

// TradingView Scanner API'den dönen üst düzey response modeli
public class TradingViewScanResponse
{
    // Filtreye uyan toplam kayıt sayısı (sayfalama için kullanılır)
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    // Her biri bir hisse senedini temsil eden satırlar
    [JsonPropertyName("data")]
    public List<TradingViewScanRow> Data { get; set; } = [];
}

// TradingView her satırı named property yerine pozisyonel dizi olarak döner.
// Örnek JSON: { "d": ["THYAO", "Türk Hava Yolları", 253.80, 1.25] }
// d[0]=sembol, d[1]=şirket adı, d[2]=kapanış fiyatı (TRY), d[3]=değişim yüzdesi
public class TradingViewScanRow
{
    // Ham pozisyonel dizi — columns sırasıyla eşleşir
    [JsonPropertyName("d")]
    public List<object?> D { get; set; } = [];

    // d[0] → hisse sembolü (örn. "THYAO")
    // JsonElement için GetString() kullanılır, ToString() ham JSON döndürür (tırnak işaretiyle)
    public string Symbol => ParseString(D, 0);

    // d[1] → şirket tam adı (örn. "Türk Hava Yolları")
    public string Description => ParseString(D, 1);

    private static string ParseString(List<object?> d, int index)
    {
        if (d.Count <= index || d[index] is null) return string.Empty;
        if (d[index] is System.Text.Json.JsonElement elem && elem.ValueKind == System.Text.Json.JsonValueKind.String)
            return elem.GetString() ?? string.Empty;
        return d[index]?.ToString() ?? string.Empty;
    }

    // d[2] → kapanış fiyatı TRY cinsinden — null gelebilir (piyasa kapalıysa vb.)
    // System.Text.Json sayıları object? içine JsonElement olarak deserialize eder,
    // bu yüzden Convert.ToDecimal yerine JsonElement.GetDecimal() kullanılır.
    public decimal? Close => ParseDecimal(D, 2);

    // d[3] → günlük değişim yüzdesi (örn. 1.25 → %1.25 artış)
    public decimal? Change => ParseDecimal(D, 3);

    private static decimal? ParseDecimal(List<object?> d, int index)
    {
        if (d.Count <= index || d[index] is null) return null;
        if (d[index] is System.Text.Json.JsonElement elem && elem.ValueKind == System.Text.Json.JsonValueKind.Number)
            return elem.GetDecimal();
        return null;
    }
}
