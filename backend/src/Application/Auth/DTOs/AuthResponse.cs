namespace PortfolioTracker.Application.Auth.DTOs;

/// <summary>
/// Login ve token refresh işlemlerinin yanıt modeli.
/// Flutter bu modeli alır; AccessToken'ı her API isteğinde header'a koyar,
/// RefreshToken'ı güvenli depoda (flutter_secure_storage) saklar.
/// ExpiresAt ile token süresi dolmadan önce proaktif refresh yapabilir.
/// </summary>
public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
