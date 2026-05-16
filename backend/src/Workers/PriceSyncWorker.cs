using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PortfolioTracker.Domain.Entities;
using PortfolioTracker.Domain.Enums;
using PortfolioTracker.Domain.Services;
using PortfolioTracker.Domain.ValueObjects;
using PortfolioTracker.Infrastructure.Data;

namespace PortfolioTracker.Workers;

public class PriceSyncWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PriceSyncWorker> _logger;
    private static readonly TimeSpan SyncInterval = TimeSpan.FromMinutes(15);

    public PriceSyncWorker(IServiceScopeFactory scopeFactory, ILogger<PriceSyncWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PriceSyncWorker başlatıldı");

        await SyncAsync(stoppingToken);

        using var timer = new PeriodicTimer(SyncInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await SyncAsync(stoppingToken);
        }
    }

    private async Task SyncAsync(CancellationToken ct)
    {
        _logger.LogInformation("Fiyat senkronizasyonu başladı");
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var provider = scope.ServiceProvider.GetRequiredService<IExchangeRateProvider>();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var currencies = await provider.GetLatestExchangeRatesAsync(ct);
            var cryptos    = await provider.GetLatestCryptoPricesAsync(ct);
            var metals     = await provider.GetLatestPreciousMetalPricesAsync(ct);
            var stocks     = await provider.GetLatestStockPricesAsync(ct);

            var incoming = new List<(string Symbol, string Name, AssetType Type, decimal PriceInTry)>();

            foreach (var r in currencies)
                incoming.Add((r.CurrencyCode.Code, r.CurrencyCode.Code, AssetType.Currency, r.GetEffectiveBuyingRate()));

            foreach (var c in cryptos)
                incoming.Add((c.Symbol, c.Symbol, AssetType.Crypto, c.PriceInTry));

            foreach (var m in metals)
            {
                var (symbol, name) = MetalInfo(m.MetalType);
                incoming.Add((symbol, name, AssetType.PreciousMetal, m.PricePerGram));
            }

            foreach (var s in stocks)
                incoming.Add((s.Symbol, s.Name, AssetType.Stock, s.PriceInTry));

            var existingAssets = await db.Assets.ToDictionaryAsync(a => a.Symbol, ct);

            foreach (var (symbol, name, type, _) in incoming)
            {
                if (!existingAssets.ContainsKey(symbol))
                    db.Assets.Add(new Asset(symbol, name, type));
            }

            await db.SaveChangesAsync(ct);

            existingAssets = await db.Assets.ToDictionaryAsync(a => a.Symbol, ct);

            var existingPrices = await db.CurrencyPrices.ToDictionaryAsync(cp => cp.AssetId, ct);
            var now = DateTime.UtcNow;

            foreach (var (symbol, _, _, priceInTry) in incoming)
            {
                if (!existingAssets.TryGetValue(symbol, out var asset)) continue;

                if (existingPrices.TryGetValue(asset.Id, out var existing))
                {
                    existing.CurrentPrice = priceInTry;
                    existing.LastUpdated  = now;
                }
                else
                {
                    db.CurrencyPrices.Add(new CurrencyPrice
                    {
                        AssetId      = asset.Id,
                        CurrentPrice = priceInTry,
                        LastUpdated  = now,
                    });
                }
            }

            await db.SaveChangesAsync(ct);

            _logger.LogInformation("Fiyat senkronizasyonu tamamlandı. {Count} varlık işlendi", incoming.Count);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fiyat senkronizasyonu sırasında hata oluştu");
        }
    }

    private static (string Symbol, string Name) MetalInfo(PreciousMetalType type) => type switch
    {
        PreciousMetalType.Gold      => ("XAU", "Altın (gram)"),
        PreciousMetalType.Silver    => ("XAG", "Gümüş (gram)"),
        PreciousMetalType.Platinum  => ("XPT", "Platin (gram)"),
        PreciousMetalType.Palladium => ("XPD", "Paladyum (gram)"),
        _                           => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
