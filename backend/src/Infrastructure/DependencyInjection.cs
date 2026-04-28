using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PortfolioTracker.Application.Auth.Services;
using PortfolioTracker.Domain.Services;
using PortfolioTracker.Infrastructure.Configuration;
using PortfolioTracker.Infrastructure.Data;
using PortfolioTracker.Infrastructure.ExternalServices.Binance;
using PortfolioTracker.Infrastructure.ExternalServices.GoldApi;
using PortfolioTracker.Infrastructure.ExternalServices.Tcmb;
using PortfolioTracker.Infrastructure.ExternalServices.TradingView;
using PortfolioTracker.Infrastructure.Services;

namespace PortfolioTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Configure TcmbOptions
        services.Configure<TcmbOptions>(
            configuration.GetSection(TcmbOptions.SectionName));

        // Configure GoldApiOptions
        services.Configure<GoldApiOptions>(
            configuration.GetSection(GoldApiOptions.SectionName));

        // Configure BinanceOptions
        services.Configure<BinanceOptions>(
            configuration.GetSection(BinanceOptions.SectionName));

        // Configure TradingViewOptions
        services.Configure<TradingViewOptions>(
            configuration.GetSection(TradingViewOptions.SectionName));

        // Register HttpClient for TCMB
        services.AddHttpClient<ITcmbClient, TcmbClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<TcmbOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        // Register HttpClient for GoldApi
        services.AddHttpClient<IGoldApiClient, GoldApiClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<GoldApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        // Register HttpClient for Binance
        services.AddHttpClient<IBinanceClient, BinanceClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<BinanceOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        // Register HttpClient for TradingView
        // ITradingViewClient → TradingViewClient bağlaması yapılır.
        // BaseAddress ve Timeout appsettings'teki TradingView section'ından okunur.
        services.AddHttpClient<ITradingViewClient, TradingViewClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<TradingViewOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        // Register memory cache
        services.AddMemoryCache();

        // Register services
        services.AddScoped<IExchangeRateProvider, CachedExchangeRateProvider>();

        // IJwtTokenService → JwtTokenService: token üretimi.
        // IAuthService → AuthService: register, login, refresh iş mantığı.
        // Her ikisi de Scoped — AppDbContext ile aynı lifetime'da olmalı.
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
