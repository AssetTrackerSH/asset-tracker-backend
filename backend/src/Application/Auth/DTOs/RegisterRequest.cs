namespace PortfolioTracker.Application.Auth.DTOs;

/// <summary>
/// POST /api/auth/register endpoint'ine gelen istek modeli.
/// record kullanılır çünkü bu veri bir kez oluşturulup okunur, değiştirilmez.
/// </summary>
public record RegisterRequest(
    string Username,
    string Email,
    string Password
);
