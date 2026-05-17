namespace PortfolioTracker.Application.Portfolio.DTOs;

public record SellResultDto(
    bool FullySold,
    decimal SoldAmount,
    decimal SellPrice,
    decimal RealizedProfitLoss,
    PortfolioItemDto? Remaining
);
