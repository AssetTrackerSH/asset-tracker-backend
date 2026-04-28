using PortfolioTracker.Application.Auth.DTOs;

namespace PortfolioTracker.Application.Auth.Services;

/// <summary>
/// Kimlik doğrulama iş mantığının sözleşmesi.
/// Controller bu interface'e bağımlıdır; JWT veya BCrypt'in nasıl çalıştığını bilmez.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Yeni kullanıcı kaydeder ve token çifti döner.
    /// Email veya username çakışırsa exception fırlatır.
    /// </summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Email veya username + şifre ile giriş yapar, token çifti döner.
    /// Kullanıcı bulunamazsa veya şifre yanlışsa exception fırlatır.
    /// </summary>
    Task<AuthResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Geçerli bir refresh token ile yeni access + refresh token çifti üretir.
    /// Eski refresh token otomatik olarak revoke edilir (tek kullanım).
    /// </summary>
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
}
