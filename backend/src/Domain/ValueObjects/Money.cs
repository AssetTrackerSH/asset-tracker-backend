namespace PortfolioTracker.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; init; }
    public CurrencyCode Currency { get; init; }

    public Money(decimal amount, CurrencyCode currency)
    {
        if (currency is null)
        {
            throw new ArgumentNullException(nameof(currency));
        }

        Amount = amount;
        Currency = currency;
    }

    public static Money Zero(CurrencyCode currency) => new(0, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException(
                $"Cannot add money with different currencies: {Currency} and {other.Currency}");
        }

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException(
                $"Cannot subtract money with different currencies: {Currency} and {other.Currency}");
        }

        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal multiplier)
    {
        return new Money(Amount * multiplier, Currency);
    }

    public Money Divide(decimal divisor)
    {
        if (divisor == 0)
        {
            throw new DivideByZeroException("Cannot divide money by zero.");
        }

        return new Money(Amount / divisor, Currency);
    }

    public override string ToString() => $"{Amount:N2} {Currency}";
}
