namespace PortfolioTracker.Application.Prices.DTOs;

public sealed record CurrencyPriceDto
{
    public string CurrencyCode { get; init; } = string.Empty;
    public decimal BuyingPrice { get; init; }
    public decimal SellingPrice { get; init; }
    public int Unit { get; init; }
}
