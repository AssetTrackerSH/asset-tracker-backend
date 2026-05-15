using PortfolioTracker.Domain.Enums;

namespace PortfolioTracker.Application.Portfolio.DTOs;

/// <summary>
/// GET /api/assets — desteklenen tüm varlıkların listesi.
/// Flutter bu listeyi "varlık ekle" ekranında dropdown olarak kullanır.
/// </summary>
public record AssetDto(
    int Id,
    string Symbol,
    string Name,
    AssetType Type
);
