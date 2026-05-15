using PortfolioTracker.Domain.Enums;

namespace PortfolioTracker.Application.Portfolio.DTOs;

/// <summary>
/// GET /api/portfolio endpoint'inin döndürdüğü her bir varlık satırı.
/// Tüm hesaplamalar backend'de yapılır; Flutter yalnızca bu DTO'yu render eder.
/// </summary>
public record PortfolioItemDto(
    Guid Id,
    int AssetId,
    string Symbol,
    string Name,
    AssetType Type,
    decimal Amount,
    decimal CurrentPrice,
    decimal TotalValue
);
