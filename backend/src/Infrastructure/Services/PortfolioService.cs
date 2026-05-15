using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PortfolioTracker.Application.Portfolio.DTOs;
using PortfolioTracker.Application.Portfolio.Services;
using PortfolioTracker.Domain.Entities;
using PortfolioTracker.Infrastructure.Data;

namespace PortfolioTracker.Infrastructure.Services;

public class PortfolioService : IPortfolioService
{
    private readonly AppDbContext _db;
    private readonly ILogger<PortfolioService> _logger;

    public PortfolioService(AppDbContext db, ILogger<PortfolioService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<PortfolioItemDto>> GetPortfolioAsync(Guid userId)
    {
        // UserPortfolios + Asset tek sorguda JOIN ile çekiyoruz.
        // AsNoTracking: sadece okuma yapıyoruz, EF Core change tracker'a gerek yok — daha hızlı.
        var items = await _db.UserPortfolios
            .AsNoTracking()
            .Where(up => up.UserId == userId)
            .Include(up => up.Asset)
            .ToListAsync();

        // Her varlık için güncel fiyatı CurrencyPrices tablosundan çek.
        // AssetId listesini önce topluyoruz, tek sorguda tüm fiyatları çekiyoruz (N+1 sorgu önlemi).
        var assetIds = items.Select(i => i.AssetId).ToList();
        var prices = await _db.CurrencyPrices
            .AsNoTracking()
            .Where(cp => assetIds.Contains(cp.AssetId))
            .ToDictionaryAsync(cp => cp.AssetId, cp => cp.CurrentPrice);

        return items.Select(up =>
        {
            // Fiyat henüz worker tarafından çekilmemişse 0 döner — Flutter "fiyat yok" gösterebilir.
            prices.TryGetValue(up.AssetId, out var currentPrice);
            return new PortfolioItemDto(
                Id: up.Id,
                AssetId: up.AssetId,
                Symbol: up.Asset.Symbol,
                Name: up.Asset.Name,
                Type: up.Asset.Type,
                Amount: up.Amount,
                CurrentPrice: currentPrice,
                TotalValue: up.Amount * currentPrice
            );
        });
    }

    public async Task<PortfolioItemDto> AddAssetAsync(Guid userId, AddAssetRequest request)
    {
        // Varlık var mı?
        var asset = await _db.Assets.FindAsync(request.AssetId)
            ?? throw new KeyNotFoundException($"AssetId={request.AssetId} bulunamadı.");

        // Kullanıcı aynı varlığı zaten eklemiş mi?
        var exists = await _db.UserPortfolios
            .AnyAsync(up => up.UserId == userId && up.AssetId == request.AssetId);
        if (exists)
            throw new InvalidOperationException($"'{asset.Symbol}' zaten portföyünüzde mevcut.");

        var portfolioItem = new UserPortfolio
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AssetId = request.AssetId,
            Amount = request.Amount,
        };

        _db.UserPortfolios.Add(portfolioItem);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Varlık eklendi: UserId={UserId}, Symbol={Symbol}, Amount={Amount}",
            userId, asset.Symbol, request.Amount);

        var currentPrice = await _db.CurrencyPrices
            .AsNoTracking()
            .Where(cp => cp.AssetId == request.AssetId)
            .Select(cp => cp.CurrentPrice)
            .FirstOrDefaultAsync();

        return new PortfolioItemDto(
            Id: portfolioItem.Id,
            AssetId: asset.Id,
            Symbol: asset.Symbol,
            Name: asset.Name,
            Type: asset.Type,
            Amount: portfolioItem.Amount,
            CurrentPrice: currentPrice,
            TotalValue: portfolioItem.Amount * currentPrice
        );
    }

    public async Task<PortfolioItemDto> UpdateAssetAsync(Guid userId, Guid portfolioItemId, UpdateAssetRequest request)
    {
        // Kayıt hem var mı hem de bu kullanıcıya mı ait?
        var item = await _db.UserPortfolios
            .Include(up => up.Asset)
            .FirstOrDefaultAsync(up => up.Id == portfolioItemId && up.UserId == userId)
            ?? throw new KeyNotFoundException("Portföy kalemi bulunamadı.");

        item.Amount = request.Amount;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Varlık güncellendi: UserId={UserId}, Symbol={Symbol}, NewAmount={Amount}",
            userId, item.Asset.Symbol, request.Amount);

        var currentPrice = await _db.CurrencyPrices
            .AsNoTracking()
            .Where(cp => cp.AssetId == item.AssetId)
            .Select(cp => cp.CurrentPrice)
            .FirstOrDefaultAsync();

        return new PortfolioItemDto(
            Id: item.Id,
            AssetId: item.AssetId,
            Symbol: item.Asset.Symbol,
            Name: item.Asset.Name,
            Type: item.Asset.Type,
            Amount: item.Amount,
            CurrentPrice: currentPrice,
            TotalValue: item.Amount * currentPrice
        );
    }

    public async Task DeleteAssetAsync(Guid userId, Guid portfolioItemId)
    {
        var item = await _db.UserPortfolios
            .FirstOrDefaultAsync(up => up.Id == portfolioItemId && up.UserId == userId)
            ?? throw new KeyNotFoundException("Portföy kalemi bulunamadı.");

        _db.UserPortfolios.Remove(item);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Varlık silindi: UserId={UserId}, PortfolioItemId={Id}", userId, portfolioItemId);
    }

    public async Task<IEnumerable<AssetDto>> GetAssetsAsync()
    {
        return await _db.Assets
            .AsNoTracking()
            .OrderBy(a => a.Type)
            .ThenBy(a => a.Symbol)
            .Select(a => new AssetDto(a.Id, a.Symbol, a.Name, a.Type))
            .ToListAsync();
    }
}
