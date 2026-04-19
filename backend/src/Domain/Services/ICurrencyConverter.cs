using PortfolioTracker.Domain.ValueObjects;

namespace PortfolioTracker.Domain.Services;

public interface ICurrencyConverter
{
    Task<Money> ConvertAsync(Money source, CurrencyCode targetCurrency, CancellationToken cancellationToken = default);
    Task<decimal> GetExchangeRateAsync(CurrencyCode from, CurrencyCode to, CancellationToken cancellationToken = default);
}
