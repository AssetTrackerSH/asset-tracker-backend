using PortfolioTracker.Domain.ValueObjects;

namespace PortfolioTracker.Domain.Entities;

public class ExchangeRate
{
    public CurrencyCode CurrencyCode { get; private set; }
    public decimal BuyingRate { get; private set; }
    public decimal SellingRate { get; private set; }
    public int Unit { get; private set; }
    public DateTime Date { get; private set; }

    public ExchangeRate(
        CurrencyCode currencyCode,
        decimal buyingRate,
        decimal sellingRate,
        int unit,
        DateTime date)
    {
        if (currencyCode is null)
        {
            throw new ArgumentNullException(nameof(currencyCode));
        }

        if (buyingRate < 0)
        {
            throw new ArgumentException("Buying rate cannot be negative.", nameof(buyingRate));
        }

        if (sellingRate < 0)
        {
            throw new ArgumentException("Selling rate cannot be negative.", nameof(sellingRate));
        }

        if (unit <= 0)
        {
            throw new ArgumentException("Unit must be greater than zero.", nameof(unit));
        }

        CurrencyCode = currencyCode;
        BuyingRate = buyingRate;
        SellingRate = sellingRate;
        Unit = unit;
        Date = date;
    }

    public decimal GetEffectiveBuyingRate() => BuyingRate / Unit;
    public decimal GetEffectiveSellingRate() => SellingRate / Unit;
}
