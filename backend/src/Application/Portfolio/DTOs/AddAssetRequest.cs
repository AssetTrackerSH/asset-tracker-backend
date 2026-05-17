namespace PortfolioTracker.Application.Portfolio.DTOs;

/// <summary>
/// POST /api/portfolio/assets — portföye yeni varlık ekle.
/// </summary>
public record AddAssetRequest(
    int AssetId,
    decimal Amount,
    decimal BuyPrice
);
