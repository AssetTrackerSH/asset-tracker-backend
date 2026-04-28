namespace PortfolioTracker.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; }

    public User User { get; set; } = null!;

    /// <summary>
    /// Token'ın süresi dolmuş mu?
    /// UTC kullanılır — sunucunun timezone'undan bağımsız doğru karşılaştırma sağlar.
    /// DateTime.Now kullansaydık, sunucu UTC+3'teyse 3 saat önce süresi dolmuş token hâlâ geçerli görünebilirdi.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Token hem süresi dolmamış hem de revoke edilmemiş ise aktiftir.
    /// — IsExpired: zaman aşımı, otomatik geçersiz olur
    /// — IsRevoked: manuel iptal (örn. logout), süre dolmadan geçersiz kılmak için
    /// İkisi birlikte olmazsa logout'tan sonra 7 gün boyunca token hâlâ geçerli sayılırdı.
    /// </summary>
    public bool IsActive => !IsRevoked && !IsExpired;
}
