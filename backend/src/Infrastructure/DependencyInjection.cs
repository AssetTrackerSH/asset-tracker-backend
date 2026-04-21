using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PortfolioTracker.Domain.Services;
using PortfolioTracker.Infrastructure.Configuration;
using PortfolioTracker.Infrastructure.ExternalServices.Binance;
using PortfolioTracker.Infrastructure.ExternalServices.GoldApi;
using PortfolioTracker.Infrastructure.ExternalServices.Tcmb;
using PortfolioTracker.Infrastructure.Services;

namespace PortfolioTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure TcmbOptions
        services.Configure<TcmbOptions>(
            configuration.GetSection(TcmbOptions.SectionName));

        // Configure GoldApiOptions
        services.Configure<GoldApiOptions>(
            configuration.GetSection(GoldApiOptions.SectionName));

        // Configure BinanceOptions
        services.Configure<BinanceOptions>(
            configuration.GetSection(BinanceOptions.SectionName));

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

        // Register memory cache
        services.AddMemoryCache();

        // Register services
        services.AddScoped<IExchangeRateProvider, CachedExchangeRateProvider>();

        return services;
    }
}
