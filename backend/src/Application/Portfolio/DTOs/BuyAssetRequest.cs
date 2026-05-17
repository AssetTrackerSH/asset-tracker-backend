namespace PortfolioTracker.Application.Portfolio.DTOs;

public record BuyAssetRequest(
    decimal Amount,
    decimal? BuyPrice
);
