using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PortfolioTracker.Domain.Entities;
using PortfolioTracker.Infrastructure.Configuration;

namespace PortfolioTracker.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

    public string GenerateAccessToken(User user)
    {
        // İmzalama için secret key'i byte dizisine çevirip HMAC-SHA256 anahtarı oluşturuyoruz.
        // HMAC-SHA256: simetrik imzalama — aynı key ile imzalanır ve doğrulanır.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Claims: token'ın içine gömdüğümüz bilgiler.
        // Backend her request'te DB'ye gitmeden bu bilgileri token'dan okuyabilir.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            // jti: her token için benzersiz ID — aynı token'ın iki kez kullanılmasını tespit etmek için
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        // Kriptografik olarak güvenli 64 byte rastgele veri üretip base64'e çeviriyoruz.
        // Guid.NewGuid() tahmin edilebilirliğe karşı yeterince güçlü değil;
        // RandomNumberGenerator ise OS'un güvenli rastgele sayı üretecini kullanır.
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
}
