namespace PortfolioTracker.Application.Portfolio.DTOs;

/// <summary>
/// PUT /api/portfolio/assets/{id} — varlık miktarını güncelle.
/// </summary>
public record UpdateAssetRequest(
    decimal Amount,
    decimal? BuyPrice
);
