namespace PortfolioTracker.Application.Prices.DTOs;

public sealed record StockPriceDto
{
    public string Symbol { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Exchange { get; init; } = string.Empty;
    public decimal PriceInUsd { get; init; }
    public decimal PriceInTry { get; init; }
}
