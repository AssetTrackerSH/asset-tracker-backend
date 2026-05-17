namespace PortfolioTracker.Application.Portfolio.DTOs;

public record SellAssetRequest(
    decimal Amount,
    decimal? SellPrice
);
