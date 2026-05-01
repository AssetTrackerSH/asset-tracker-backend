using PortfolioTracker.Domain.Entities;

namespace PortfolioTracker.Infrastructure.Services;

/// <summary>
/// JWT access token ve refresh token üretiminden sorumlu servis sözleşmesi.
/// Application katmanı bu interface'i kullanır; JWT'nin nasıl üretildiğini bilmez.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Kullanıcı bilgilerinden imzalı bir JWT access token üretir.
    /// Token süresi JwtSettings.AccessTokenExpirationMinutes ile belirlenir.
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Kriptografik olarak güvenli rastgele bir refresh token üretir.
    /// Token DB'ye kaydedilir; doğrulanırken DB'deki kayıtla karşılaştırılır.
    /// </summary>
    string GenerateRefreshToken();
}
