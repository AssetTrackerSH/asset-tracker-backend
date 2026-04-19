using Microsoft.Extensions.DependencyInjection;
using PortfolioTracker.Application.Prices.Services;
using PortfolioTracker.Domain.Services;

namespace PortfolioTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<IPriceService, PriceService>();

        // Register domain services
        services.AddScoped<ICurrencyConverter, CurrencyConverter>();

        return services;
    }
}
