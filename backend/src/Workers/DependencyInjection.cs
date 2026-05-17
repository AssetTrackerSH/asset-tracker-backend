using Microsoft.Extensions.DependencyInjection;

namespace PortfolioTracker.Workers;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkers(this IServiceCollection services)
    {
        services.AddHostedService<PriceSyncWorker>();
        return services;
    }
}
