using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Application.Auth.DTOs;
using PortfolioTracker.Application.Auth.Services;

namespace PortfolioTracker.Api.Endpoints;

public static class AuthEndpoints
{
    // Extension method — Program.cs'de app.MapAuthEndpoints() şeklinde çağrılır.
    // "this IEndpointRouteBuilder app" parametresi bu metodu IEndpointRouteBuilder'a bağlar;
    // böylece app.MapGroup gibi metodlarla aynı sözdiziminde kullanılır.
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // MapGroup: tüm auth endpoint'leri "/api/auth" prefix'i altında gruplar.
        // Her endpoint'e tek tek "/api/auth" yazmak yerine bir kez burada tanımlanır.
        // WithTags("Auth"): Swagger UI'da endpoint'leri "Auth" başlığı altında gösterir.
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        // POST /api/auth/register → RegisterHandler metodunu çağırır.
        // WithName: bu endpoint'e isim verir — Swagger ve redirect için kullanılır.
        // WithSummary: Swagger'da kısa açıklama olarak görünür.
        // Produces: Swagger'a "bu endpoint hangi HTTP status'larını döner?" bilgisini verir.
        group.MapPost("/register", RegisterHandler)
            .WithName("Register")
            .WithSummary("Yeni kullanıcı kaydı")
            .Produces<AuthResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapPost("/login", LoginHandler)
            .WithName("Login")
            .WithSummary("Kullanıcı girişi")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapPost("/refresh", RefreshHandler)
            .WithName("RefreshToken")
            .WithSummary("Access token yenile")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        return app;
    }

    // RegisterHandler: POST /api/auth/register
    // "RegisterRequest request" — ASP.NET, gelen JSON body'yi otomatik olarak bu nesneye deserialize eder.
    // "IAuthService authService" — DI container bu parametreyi otomatik inject eder; new() yazmak gerekmez.
    private static async Task<IResult> RegisterHandler(
        RegisterRequest request,
        IAuthService authService)
    {
        try
        {
            var response = await authService.RegisterAsync(request);

            // Results.Created: HTTP 201 döner + Location header'a endpoint URL'i yazar.
            // 201, "kaynak oluşturuldu" anlamına gelir; register için 200'den daha doğru.
            return Results.Created("/api/auth/login", response);
        }
        catch (InvalidOperationException ex)
        {
            // Email veya username çakışması — AuthService bu exception'ı fırlatıyor.
            // 409 Conflict: "bu kaynak zaten var" anlamına gelir, 400'den daha semantik.
            return Results.Conflict(new ProblemDetails
            {
                Title = "Kayıt Başarısız",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
        catch (Exception)
        {
            return Results.Problem(
                title: "Sunucu Hatası",
                detail: "Kayıt sırasında beklenmeyen bir hata oluştu.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    // LoginHandler: POST /api/auth/login
    private static async Task<IResult> LoginHandler(
        LoginRequest request,
        IAuthService authService)
    {
        try
        {
            var response = await authService.LoginAsync(request);

            // Results.Ok: HTTP 200 + JSON body döner.
            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            // Email ve username ikisi de boş geldi — AuthService ArgumentException fırlatıyor.
            // 400 Bad Request: istek formatı hatalı.
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Geçersiz İstek",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (UnauthorizedAccessException)
        {
            // Kullanıcı bulunamadı veya şifre yanlış — AuthService UnauthorizedAccessException fırlatıyor.
            // 401 Unauthorized: kimlik doğrulama başarısız.
            return Results.Unauthorized();
        }
        catch (Exception)
        {
            return Results.Problem(
                title: "Sunucu Hatası",
                detail: "Giriş sırasında beklenmeyen bir hata oluştu.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    // RefreshHandler: POST /api/auth/refresh
    // Body'den sadece refresh token string'i alıyoruz — bunun için küçük bir record kullanıyoruz.
    // Alternatif: query string veya header'dan alınabilirdi, ama body daha güvenli (HTTPS ile şifrelenir).
    private static async Task<IResult> RefreshHandler(
        RefreshTokenRequest request,
        IAuthService authService)
    {
        try
        {
            var response = await authService.RefreshTokenAsync(request.RefreshToken);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            // Token bulunamadı, süresi dolmuş veya revoke edilmiş.
            return Results.Unauthorized();
        }
        catch (Exception)
        {
            return Results.Problem(
                title: "Sunucu Hatası",
                detail: "Token yenileme sırasında beklenmeyen bir hata oluştu.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}

// Refresh endpoint'ine gelen body modeli.
// Ayrı bir DTO dosyası açmak yerine burada tanımladık — sadece bu endpoint kullanıyor.
public record RefreshTokenRequest(string RefreshToken);
