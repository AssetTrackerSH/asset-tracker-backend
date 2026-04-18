namespace PortfolioTracker.Application.Prices.DTOs;

public sealed record PreciousMetalPriceDto
{
    public string MetalType { get; init; } = string.Empty;
    public decimal PricePerGram { get; init; }
    public decimal PricePerOunce { get; init; }
}
