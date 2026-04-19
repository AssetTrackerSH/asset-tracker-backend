using PortfolioTracker.Domain.ValueObjects;

namespace PortfolioTracker.Domain.Entities;

public class PreciousMetalPrice
{
    public PreciousMetalType MetalType { get; private set; }
    public decimal PricePerGram { get; private set; }
    public decimal PricePerOunce { get; private set; }
    public DateTime Date { get; private set; }

    public PreciousMetalPrice(
        PreciousMetalType metalType,
        decimal pricePerGram,
        decimal pricePerOunce,
        DateTime date)
    {
        if (pricePerGram < 0)
        {
            throw new ArgumentException("Price per gram cannot be negative.", nameof(pricePerGram));
        }

        if (pricePerOunce < 0)
        {
            throw new ArgumentException("Price per ounce cannot be negative.", nameof(pricePerOunce));
        }

        MetalType = metalType;
        PricePerGram = pricePerGram;
        PricePerOunce = pricePerOunce;
        Date = date;
    }
}
