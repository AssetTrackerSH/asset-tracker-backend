namespace PortfolioTracker.Application.Prices.DTOs;

public sealed record PriceResponseDto
{
    public string BaseCurrency { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public IReadOnlyList<CurrencyPriceDto> Currencies { get; init; } = Array.Empty<CurrencyPriceDto>();
    public IReadOnlyList<PreciousMetalPriceDto> PreciousMetals { get; init; } = Array.Empty<PreciousMetalPriceDto>();
    public IReadOnlyList<CryptoPriceDto> Cryptos { get; init; } = Array.Empty<CryptoPriceDto>();
    public IReadOnlyList<StockPriceDto> Stocks { get; init; } = Array.Empty<StockPriceDto>();
}
