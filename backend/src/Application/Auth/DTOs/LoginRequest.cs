namespace PortfolioTracker.Application.Auth.DTOs;

/// <summary>
/// POST /api/auth/login endpoint'ine gelen istek modeli.
/// Email veya Username'den biri zorunludur; ikisi birden null olamaz.
/// Bu kontrolü DTO değil, AuthService yapar — DTO sadece veriyi taşır.
/// </summary>
public record LoginRequest(
    string? Email,
    string? Username,
    string Password
);
