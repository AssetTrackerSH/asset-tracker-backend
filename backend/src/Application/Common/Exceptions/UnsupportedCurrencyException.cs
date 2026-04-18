namespace PortfolioTracker.Application.Common.Exceptions;

public class UnsupportedCurrencyException : Exception
{
    public UnsupportedCurrencyException(string currencyCode)
        : base($"Currency '{currencyCode}' is not supported.")
    {
    }

    public UnsupportedCurrencyException(string currencyCode, Exception innerException)
        : base($"Currency '{currencyCode}' is not supported.", innerException)
    {
    }
}
