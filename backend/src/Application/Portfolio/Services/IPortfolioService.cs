using PortfolioTracker.Application.Portfolio.DTOs;

namespace PortfolioTracker.Application.Portfolio.Services;

public interface IPortfolioService
{
    /// <summary>
    /// Kullanıcının tüm portföy kalemlerini fiyatlarıyla birlikte döner.
    /// Amount * CurrentPrice hesaplaması burada yapılır.
    /// </summary>
    Task<IEnumerable<PortfolioItemDto>> GetPortfolioAsync(Guid userId);

    /// <summary>
    /// Portföye yeni varlık ekler.
    /// Aynı varlık zaten varsa exception fırlatır.
    /// </summary>
    Task<PortfolioItemDto> AddAssetAsync(Guid userId, AddAssetRequest request);

    /// <summary>
    /// Mevcut varlığın miktarını günceller.
    /// Kayıt bulunamazsa veya başka kullanıcıya aitse exception fırlatır.
    /// </summary>
    Task<PortfolioItemDto> UpdateAssetAsync(Guid userId, Guid portfolioItemId, UpdateAssetRequest request);

    /// <summary>
    /// Portföyden varlık siler.
    /// Kayıt bulunamazsa veya başka kullanıcıya aitse exception fırlatır.
    /// </summary>
    Task DeleteAssetAsync(Guid userId, Guid portfolioItemId);

    /// <summary>
    /// Desteklenen tüm varlıkları döner (seed data).
    /// Flutter "varlık ekle" ekranında bu listeyi kullanır.
    /// </summary>
    Task<IEnumerable<AssetDto>> GetAssetsAsync();
}
