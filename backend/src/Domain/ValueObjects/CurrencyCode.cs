namespace PortfolioTracker.Domain.ValueObjects;

public sealed class CurrencyCode : ValueObject
{
    public string Code { get; }

    private CurrencyCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Currency code cannot be null or empty.", nameof(code));
        }

        if (code.Length != 3)
        {
            throw new ArgumentException("Currency code must be exactly 3 characters.", nameof(code));
        }

        if (!code.All(char.IsLetter))
        {
            throw new ArgumentException("Currency code must contain only letters.", nameof(code));
        }

        Code = code.ToUpperInvariant();
    }

    public static CurrencyCode From(string code) => new(code);

    // Common currency codes
    public static CurrencyCode TRY => new("TRY");
    public static CurrencyCode USD => new("USD");
    public static CurrencyCode EUR => new("EUR");
    public static CurrencyCode GBP => new("GBP");
    public static CurrencyCode JPY => new("JPY");
    public static CurrencyCode CHF => new("CHF");
    public static CurrencyCode CAD => new("CAD");
    public static CurrencyCode AUD => new("AUD");
    public static CurrencyCode CNY => new("CNY");
    public static CurrencyCode RUB => new("RUB");
    public static CurrencyCode SAR => new("SAR");
    public static CurrencyCode AED => new("AED");
    public static CurrencyCode SEK => new("SEK");
    public static CurrencyCode NOK => new("NOK");
    public static CurrencyCode DKK => new("DKK");

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
    }

    public override string ToString() => Code;

    public static implicit operator string(CurrencyCode currencyCode) => currencyCode.Code;
}
