namespace PortfolioTracker.Application.Portfolio.DTOs;

public record PortfolioSummaryDto(
    decimal TotalValue,
    decimal TotalCostBasis,
    decimal NetProfitLoss,
    decimal ReturnPercent
);
