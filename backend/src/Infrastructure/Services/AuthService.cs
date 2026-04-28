using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortfolioTracker.Application.Auth.DTOs;
using PortfolioTracker.Application.Auth.Services;
using PortfolioTracker.Domain.Entities;
using PortfolioTracker.Infrastructure.Configuration;
using PortfolioTracker.Infrastructure.Data;

namespace PortfolioTracker.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext db,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Email veya username daha önce alınmış mı kontrol et.
        // AnyAsync: tüm kaydı çekmek yerine sadece "var mı?" sorusunu DB'e sormak daha hızlı.
        var emailExists = await _db.Users.AnyAsync(u => u.Email == request.Email);
        if (emailExists)
            throw new InvalidOperationException($"'{request.Email}' adresi zaten kullanımda.");

        var usernameExists = await _db.Users.AnyAsync(u => u.Username == request.Username);
        if (usernameExists)
            throw new InvalidOperationException($"'{request.Username}' kullanıcı adı zaten alınmış.");

        // BCrypt.EnhancedHashPassword: şifreyi tuzlayıp hash'ler.
        // "Enhanced" versiyonu şifreyi normalize eder; uzun şifrelerde (>72 byte) truncation olmuyor.
        // WorkFactor 12: 2^12 iterasyon — brute-force maliyetli olur, register başına ~250ms kabul edilebilir.
        var passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password, workFactor: 12);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
        };

        _db.Users.Add(user);

        // Refresh token üret ve DB'ye kaydet.
        // Register sonrası kullanıcı direkt giriş yapmış sayılır — ayrı login gerektirmez.
        var refreshToken = CreateRefreshToken(user.Id);
        _db.RefreshTokens.Add(refreshToken);

        await _db.SaveChangesAsync();

        _logger.LogInformation("Yeni kullanıcı kaydedildi: {Username} ({UserId})", user.Username, user.Id);

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        return new AuthResponse(accessToken, refreshToken.Token, DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Email veya username'den biri zorunlu — ikisi de null olamaz.
        if (string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.Username))
            throw new ArgumentException("Email veya kullanıcı adı girilmelidir.");

        // Kullanıcıyı email veya username ile bul.
        // FirstOrDefaultAsync: bulunamazsa null döner, exception fırlatmaz.
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            (!string.IsNullOrEmpty(request.Email) && u.Email == request.Email) ||
            (!string.IsNullOrEmpty(request.Username) && u.Username == request.Username));

        // "Kullanıcı bulunamadı" veya "şifre yanlış" durumunda aynı mesajı veriyoruz.
        // Farklı mesaj versek saldırgan hangi email'in kayıtlı olduğunu öğrenebilir (user enumeration).
        if (user is null || !BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Başarısız giriş denemesi: {EmailOrUsername}", request.Email ?? request.Username);
            throw new UnauthorizedAccessException("Email/kullanıcı adı veya şifre hatalı.");
        }

        // Bu kullanıcının mevcut aktif refresh token'larını revoke et.
        // Böylece aynı hesap aynı anda birden fazla aktif refresh token'a sahip olmaz.
        // (Tek cihaz politikası — isterseniz bu satırı kaldırıp çok cihaz desteği açılabilir.)
        var activeTokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == user.Id && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var token in activeTokens)
            token.IsRevoked = true;

        var refreshToken = CreateRefreshToken(user.Id);
        _db.RefreshTokens.Add(refreshToken);

        await _db.SaveChangesAsync();

        _logger.LogInformation("Kullanıcı giriş yaptı: {Username} ({UserId})", user.Username, user.Id);

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        return new AuthResponse(accessToken, refreshToken.Token, DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes));
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        // Refresh token DB'de var mı ve hâlâ geçerli mi?
        // Include: token ile birlikte User'ı da çekiyoruz — access token üretmek için kullanıcıya ihtiyaç var.
        var storedToken = await _db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            _logger.LogWarning("Geçersiz veya süresi dolmuş refresh token kullanım denemesi.");
            throw new UnauthorizedAccessException("Refresh token geçersiz veya süresi dolmuş.");
        }

        // Rotation pattern: eski token'ı revoke et, yeni token çifti üret.
        // Aynı refresh token tekrar kullanılırsa (token çalınmış olabilir) geçersiz olur.
        storedToken.IsRevoked = true;

        var newRefreshToken = CreateRefreshToken(storedToken.UserId);
        _db.RefreshTokens.Add(newRefreshToken);

        await _db.SaveChangesAsync();

        _logger.LogInformation("Token yenilendi: {UserId}", storedToken.UserId);

        var accessToken = _jwtTokenService.GenerateAccessToken(storedToken.User);
        return new AuthResponse(accessToken, newRefreshToken.Token, DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes));
    }

    private RefreshToken CreateRefreshToken(Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Token = _jwtTokenService.GenerateRefreshToken(),
        ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
        CreatedAt = DateTime.UtcNow,
        IsRevoked = false,
    };
}
