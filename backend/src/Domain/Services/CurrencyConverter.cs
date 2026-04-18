using PortfolioTracker.Domain.Entities;
using PortfolioTracker.Domain.ValueObjects;

namespace PortfolioTracker.Domain.Services;

public class CurrencyConverter : ICurrencyConverter
{
    private readonly IExchangeRateProvider _exchangeRateProvider;

    public CurrencyConverter(IExchangeRateProvider exchangeRateProvider)
    {
        _exchangeRateProvider = exchangeRateProvider ?? throw new ArgumentNullException(nameof(exchangeRateProvider));
    }

    public async Task<Money> ConvertAsync(Money source, CurrencyCode targetCurrency, CancellationToken cancellationToken = default)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (targetCurrency is null)
        {
            throw new ArgumentNullException(nameof(targetCurrency));
        }

        if (source.Currency == targetCurrency)
        {
            return source;
        }

        var exchangeRate = await GetExchangeRateAsync(source.Currency, targetCurrency, cancellationToken);
        var convertedAmount = source.Amount * exchangeRate;

        return new Money(convertedAmount, targetCurrency);
    }

    public async Task<decimal> GetExchangeRateAsync(CurrencyCode from, CurrencyCode to, CancellationToken cancellationToken = default)
    {
        if (from is null)
        {
            throw new ArgumentNullException(nameof(from));
        }

        if (to is null)
        {
            throw new ArgumentNullException(nameof(to));
        }

        if (from == to)
        {
            return 1m;
        }

        var rates = await _exchangeRateProvider.GetLatestExchangeRatesAsync(cancellationToken);

        // TRY is the pivot currency (TCMB provides all rates against TRY)
        var tryCode = CurrencyCode.TRY;

        // If converting from TRY to another currency
        if (from == tryCode)
        {
            var toRate = rates.FirstOrDefault(r => r.CurrencyCode == to)
                ?? throw new InvalidOperationException($"Exchange rate not found for currency: {to}");

            // 1 TRY = 1 / (foreign currency rate) of the foreign currency
            // For example: If USD/TRY = 34, then TRY/USD = 1/34
            return 1m / toRate.GetEffectiveBuyingRate();
        }

        // If converting to TRY from another currency
        if (to == tryCode)
        {
            var fromRate = rates.FirstOrDefault(r => r.CurrencyCode == from)
                ?? throw new InvalidOperationException($"Exchange rate not found for currency: {from}");

            // Direct rate (e.g., USD/TRY = 34 means 1 USD = 34 TRY)
            return fromRate.GetEffectiveBuyingRate();
        }

        // Cross-currency conversion via TRY
        // Example: USD to EUR
        // 1. Get USD/TRY rate (e.g., 34)
        // 2. Get EUR/TRY rate (e.g., 37)
        // 3. USD/EUR = (USD/TRY) / (EUR/TRY) = 34/37 = 0.919

        var fromToTryRate = rates.FirstOrDefault(r => r.CurrencyCode == from)
            ?? throw new InvalidOperationException($"Exchange rate not found for currency: {from}");

        var toToTryRate = rates.FirstOrDefault(r => r.CurrencyCode == to)
            ?? throw new InvalidOperationException($"Exchange rate not found for currency: {to}");

        var crossRate = fromToTryRate.GetEffectiveBuyingRate() / toToTryRate.GetEffectiveBuyingRate();

        return crossRate;
    }
}
