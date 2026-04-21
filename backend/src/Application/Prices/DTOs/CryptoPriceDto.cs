namespace PortfolioTracker.Application.Prices.DTOs;

public sealed record CryptoPriceDto
{
    public string Symbol { get; init; } = string.Empty;
    public decimal PriceInUsd { get; init; }
    public decimal PriceInTry { get; init; }
}
