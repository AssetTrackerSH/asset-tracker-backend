namespace PortfolioTracker.Domain.Entities;

public class CryptoPrice
{
    public string Symbol { get; private set; }
    public decimal PriceInUsd { get; private set; }
    public decimal PriceInTry { get; private set; }
    public DateTime Date { get; private set; }

    public CryptoPrice(string symbol, decimal priceInUsd, decimal priceInTry, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));

        if (priceInUsd < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(priceInUsd));

        if (priceInTry < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(priceInTry));

        Symbol = symbol.ToUpperInvariant();
        PriceInUsd = priceInUsd;
        PriceInTry = priceInTry;
        Date = date;
    }
}
