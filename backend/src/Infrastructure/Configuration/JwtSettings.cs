namespace PortfolioTracker.Infrastructure.Configuration;

/// <summary>
/// appsettings.json içindeki "JwtSettings" section'ını strongly-typed olarak okur.
/// Magic string yerine bu class inject edilir; böylece yazım hatası derleme zamanında yakalanır.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// appsettings.json'daki section adı.
    /// DI kaydında configuration.GetSection(JwtSettings.SectionName) şeklinde kullanılır;
    /// section adını tek yerden yönetmek için sabit tutulur.
    /// </summary>
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// JWT imzalama anahtarı. En az 32 karakter olmalı.
    /// Kaynak koda yazılmaz — appsettings.Development.json veya environment variable'dan gelir.
    /// </summary>
    public string SecretKey { get; init; } = string.Empty;

    /// <summary>
    /// Token'ı kimin ürettiği. Genellikle uygulama adı.
    /// Token doğrulanırken bu değer kontrol edilir.
    /// </summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>
    /// Token'ın hangi uygulama/servis için üretildiği.
    /// Token doğrulanırken bu değer kontrol edilir.
    /// </summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// Access token geçerlilik süresi (dakika). Varsayılan: 60.
    /// Kısa tutulur — çalınsa bile kısa sürede geçersiz kalır.
    /// </summary>
    public int AccessTokenExpirationMinutes { get; init; } = 60;

    /// <summary>
    /// Refresh token geçerlilik süresi (gün). Varsayılan: 7.
    /// Kullanıcıyı yeniden login'e zorlamadan yeni access token üretmek için kullanılır.
    /// </summary>
    public int RefreshTokenExpirationDays { get; init; } = 7;
}
