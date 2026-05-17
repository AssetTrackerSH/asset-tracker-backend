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
            prices.TryGetValue(up.AssetId, out var currentPrice);
            var totalValue = up.Amount * currentPrice;
            var costBasis = up.Amount * up.BuyPrice;
            var profitLoss = totalValue - costBasis;
            var profitLossPercent = costBasis == 0 ? 0 : profitLoss / costBasis * 100;
            return new PortfolioItemDto(
                Id: up.Id,
                AssetId: up.AssetId,
                Symbol: up.Asset.Symbol,
                Name: up.Asset.Name,
                Type: up.Asset.Type,
                Amount: up.Amount,
                BuyPrice: up.BuyPrice,
                CurrentPrice: currentPrice,
                CostBasis: costBasis,
                TotalValue: totalValue,
                ProfitLoss: profitLoss,
                ProfitLossPercent: profitLossPercent
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
            BuyPrice = request.BuyPrice,
        };

        _db.UserPortfolios.Add(portfolioItem);
        _db.Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AssetId = request.AssetId,
            Type = TransactionType.Buy,
            Amount = request.Amount,
            Price = request.BuyPrice,
            TotalValue = request.Amount * request.BuyPrice,
            Date = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();

        _logger.LogInformation("Varlık eklendi: UserId={UserId}, Symbol={Symbol}, Amount={Amount}",
            userId, asset.Symbol, request.Amount);

        var currentPrice = await _db.CurrencyPrices
            .AsNoTracking()
            .Where(cp => cp.AssetId == request.AssetId)
            .Select(cp => cp.CurrentPrice)
            .FirstOrDefaultAsync();

        var totalValue = portfolioItem.Amount * currentPrice;
        var costBasis = portfolioItem.Amount * portfolioItem.BuyPrice;
        var profitLoss = totalValue - costBasis;
        var profitLossPercent = costBasis == 0 ? 0 : profitLoss / costBasis * 100;

        return new PortfolioItemDto(
            Id: portfolioItem.Id,
            AssetId: asset.Id,
            Symbol: asset.Symbol,
            Name: asset.Name,
            Type: asset.Type,
            Amount: portfolioItem.Amount,
            BuyPrice: portfolioItem.BuyPrice,
            CurrentPrice: currentPrice,
            CostBasis: costBasis,
            TotalValue: totalValue,
            ProfitLoss: profitLoss,
            ProfitLossPercent: profitLossPercent
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
        if (request.BuyPrice.HasValue)
            item.BuyPrice = request.BuyPrice.Value;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Varlık güncellendi: UserId={UserId}, Symbol={Symbol}, NewAmount={Amount}",
            userId, item.Asset.Symbol, request.Amount);

        var currentPrice = await _db.CurrencyPrices
            .AsNoTracking()
            .Where(cp => cp.AssetId == item.AssetId)
            .Select(cp => cp.CurrentPrice)
            .FirstOrDefaultAsync();

        var totalValue = item.Amount * currentPrice;
        var costBasis = item.Amount * item.BuyPrice;
        var profitLoss = totalValue - costBasis;
        var profitLossPercent = costBasis == 0 ? 0 : profitLoss / costBasis * 100;

        return new PortfolioItemDto(
            Id: item.Id,
            AssetId: item.AssetId,
            Symbol: item.Asset.Symbol,
            Name: item.Asset.Name,
            Type: item.Asset.Type,
            Amount: item.Amount,
            BuyPrice: item.BuyPrice,
            CurrentPrice: currentPrice,
            CostBasis: costBasis,
            TotalValue: totalValue,
            ProfitLoss: profitLoss,
            ProfitLossPercent: profitLossPercent
        );
    }

    public async Task<PortfolioItemDto> BuyMoreAsync(Guid userId, Guid portfolioItemId, BuyAssetRequest request)
    {
        var item = await _db.UserPortfolios
            .Include(up => up.Asset)
            .FirstOrDefaultAsync(up => up.Id == portfolioItemId && up.UserId == userId)
            ?? throw new KeyNotFoundException("Portföy kalemi bulunamadı.");

        var newBuyPrice = request.BuyPrice ?? item.BuyPrice;
        var totalCost = item.Amount * item.BuyPrice + request.Amount * newBuyPrice;
        item.Amount += request.Amount;
        item.BuyPrice = item.Amount == 0 ? 0 : totalCost / item.Amount;

        _db.Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AssetId = item.AssetId,
            Type = TransactionType.Buy,
            Amount = request.Amount,
            Price = newBuyPrice,
            TotalValue = request.Amount * newBuyPrice,
            Date = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();

        _logger.LogInformation("Ek alım yapıldı: UserId={UserId}, Symbol={Symbol}, AddedAmount={Amount}, NewAvgBuyPrice={BuyPrice}",
            userId, item.Asset.Symbol, request.Amount, item.BuyPrice);

        var currentPrice = await _db.CurrencyPrices
            .AsNoTracking()
            .Where(cp => cp.AssetId == item.AssetId)
            .Select(cp => cp.CurrentPrice)
            .FirstOrDefaultAsync();

        var totalValue = item.Amount * currentPrice;
        var costBasis = item.Amount * item.BuyPrice;
        var profitLoss = totalValue - costBasis;
        var profitLossPercent = costBasis == 0 ? 0 : profitLoss / costBasis * 100;

        return new PortfolioItemDto(
            Id: item.Id,
            AssetId: item.AssetId,
            Symbol: item.Asset.Symbol,
            Name: item.Asset.Name,
            Type: item.Asset.Type,
            Amount: item.Amount,
            BuyPrice: item.BuyPrice,
            CurrentPrice: currentPrice,
            CostBasis: costBasis,
            TotalValue: totalValue,
            ProfitLoss: profitLoss,
            ProfitLossPercent: profitLossPercent
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

    public async Task<SellResultDto> SellAssetAsync(Guid userId, Guid portfolioItemId, SellAssetRequest request)
    {
        var item = await _db.UserPortfolios
            .Include(up => up.Asset)
            .FirstOrDefaultAsync(up => up.Id == portfolioItemId && up.UserId == userId)
            ?? throw new KeyNotFoundException("Portföy kalemi bulunamadı.");

        if (request.Amount <= 0 || request.Amount > item.Amount)
            throw new InvalidOperationException($"Geçersiz satış miktarı. Mevcut: {item.Amount}");

        var currentPrice = await _db.CurrencyPrices
            .AsNoTracking()
            .Where(cp => cp.AssetId == item.AssetId)
            .Select(cp => cp.CurrentPrice)
            .FirstOrDefaultAsync();

        var sellPrice = request.SellPrice ?? currentPrice;
        var realizedProfitLoss = (sellPrice - item.BuyPrice) * request.Amount;
        var fullySold = request.Amount == item.Amount;

        if (fullySold)
        {
            _db.UserPortfolios.Remove(item);
            _db.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AssetId = item.AssetId,
                Type = TransactionType.Sell,
                Amount = request.Amount,
                Price = sellPrice,
                TotalValue = request.Amount * sellPrice,
                Date = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync();

            _logger.LogInformation("Tam satış: UserId={UserId}, Symbol={Symbol}, Amount={Amount}, SellPrice={SellPrice}, PnL={PnL}",
                userId, item.Asset.Symbol, request.Amount, sellPrice, realizedProfitLoss);

            return new SellResultDto(
                FullySold: true,
                SoldAmount: request.Amount,
                SellPrice: sellPrice,
                RealizedProfitLoss: realizedProfitLoss,
                Remaining: null
            );
        }

        item.Amount -= request.Amount;
        _db.Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AssetId = item.AssetId,
            Type = TransactionType.Sell,
            Amount = request.Amount,
            Price = sellPrice,
            TotalValue = request.Amount * sellPrice,
            Date = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();

        _logger.LogInformation("Kısmi satış: UserId={UserId}, Symbol={Symbol}, SoldAmount={Amount}, Remaining={Remaining}, SellPrice={SellPrice}, PnL={PnL}",
            userId, item.Asset.Symbol, request.Amount, item.Amount, sellPrice, realizedProfitLoss);

        var remainingValue = item.Amount * currentPrice;
        var remainingCost = item.Amount * item.BuyPrice;
        var remainingPnl = remainingValue - remainingCost;
        var remainingPnlPct = remainingCost == 0 ? 0 : remainingPnl / remainingCost * 100;

        return new SellResultDto(
            FullySold: false,
            SoldAmount: request.Amount,
            SellPrice: sellPrice,
            RealizedProfitLoss: realizedProfitLoss,
            Remaining: new PortfolioItemDto(
                Id: item.Id,
                AssetId: item.AssetId,
                Symbol: item.Asset.Symbol,
                Name: item.Asset.Name,
                Type: item.Asset.Type,
                Amount: item.Amount,
                BuyPrice: item.BuyPrice,
                CurrentPrice: currentPrice,
                CostBasis: remainingCost,
                TotalValue: remainingValue,
                ProfitLoss: remainingPnl,
                ProfitLossPercent: remainingPnlPct
            )
        );
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsAsync(Guid userId, Guid portfolioItemId)
    {
        var item = await _db.UserPortfolios
            .AsNoTracking()
            .FirstOrDefaultAsync(up => up.Id == portfolioItemId && up.UserId == userId)
            ?? throw new KeyNotFoundException("Portföy kalemi bulunamadı.");

        return await _db.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.AssetId == item.AssetId)
            .Include(t => t.Asset)
            .OrderByDescending(t => t.Date)
            .Select(t => new TransactionDto(t.Id, t.Asset.Symbol, t.Asset.Name, t.Type, t.Amount, t.Price, t.TotalValue, t.Date))
            .ToListAsync();
    }

    public async Task<PortfolioSummaryDto> GetSummaryAsync(Guid userId)
    {
        var items = await _db.UserPortfolios
            .AsNoTracking()
            .Where(up => up.UserId == userId)
            .ToListAsync();

        if (!items.Any())
            return new PortfolioSummaryDto(0, 0, 0, 0);

        var assetIds = items.Select(i => i.AssetId).ToList();
        var prices = await _db.CurrencyPrices
            .AsNoTracking()
            .Where(cp => assetIds.Contains(cp.AssetId))
            .ToDictionaryAsync(cp => cp.AssetId, cp => cp.CurrentPrice);

        var totalValue = items.Sum(up => {
            prices.TryGetValue(up.AssetId, out var price);
            return up.Amount * price;
        });
        var totalCostBasis = items.Sum(up => up.Amount * up.BuyPrice);
        var netProfitLoss = totalValue - totalCostBasis;
        var returnPercent = totalCostBasis == 0 ? 0 : netProfitLoss / totalCostBasis * 100;

        return new PortfolioSummaryDto(totalValue, totalCostBasis, netProfitLoss, returnPercent);
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
