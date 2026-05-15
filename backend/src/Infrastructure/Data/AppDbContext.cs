using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Domain.Entities;
using PortfolioTracker.Domain.Enums;

namespace PortfolioTracker.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<UserPortfolio> UserPortfolios => Set<UserPortfolio>();
    public DbSet<CurrencyPrice> CurrencyPrices => Set<CurrencyPrice>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Username).IsRequired().HasMaxLength(50);
            e.Property(u => u.Email).IsRequired().HasMaxLength(100);
            e.Property(u => u.PasswordHash).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<Asset>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Symbol).IsRequired().HasMaxLength(20);
            e.Property(a => a.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(a => a.Symbol).IsUnique();

            // Seed data: desteklenen tüm varlıklar.
            // Id'ler sabit — migration'lar arası tutarlılık için elle belirlendi.
            e.HasData(
                // Döviz
                new { Id = 1,  Symbol = "USD", Name = "Amerikan Doları",   Type = AssetType.Currency },
                new { Id = 2,  Symbol = "EUR", Name = "Euro",              Type = AssetType.Currency },
                new { Id = 3,  Symbol = "GBP", Name = "İngiliz Sterlini",  Type = AssetType.Currency },
                new { Id = 4,  Symbol = "CHF", Name = "İsviçre Frangı",    Type = AssetType.Currency },
                new { Id = 5,  Symbol = "JPY", Name = "Japon Yeni",        Type = AssetType.Currency },

                // Kripto
                new { Id = 6,  Symbol = "BTC", Name = "Bitcoin",           Type = AssetType.Crypto },
                new { Id = 7,  Symbol = "ETH", Name = "Ethereum",          Type = AssetType.Crypto },
                new { Id = 8,  Symbol = "BNB", Name = "BNB",               Type = AssetType.Crypto },
                new { Id = 9,  Symbol = "SOL", Name = "Solana",            Type = AssetType.Crypto },
                new { Id = 10, Symbol = "XRP", Name = "XRP",               Type = AssetType.Crypto },

                // Altın / Gümüş
                new { Id = 11, Symbol = "XAU", Name = "Altın (gram)",      Type = AssetType.PreciousMetal },
                new { Id = 12, Symbol = "XAG", Name = "Gümüş (gram)",      Type = AssetType.PreciousMetal },

                // BIST Hisse Senetleri
                new { Id = 13, Symbol = "THYAO", Name = "Türk Hava Yolları",       Type = AssetType.Stock },
                new { Id = 14, Symbol = "GARAN", Name = "Garanti BBVA",            Type = AssetType.Stock },
                new { Id = 15, Symbol = "ASELS", Name = "Aselsan",                 Type = AssetType.Stock },
                new { Id = 16, Symbol = "EREGL", Name = "Ereğli Demir Çelik",      Type = AssetType.Stock },
                new { Id = 17, Symbol = "SISE",  Name = "Şişe Cam",                Type = AssetType.Stock }
            );
        });

        modelBuilder.Entity<UserPortfolio>(e =>
        {
            e.HasKey(up => up.Id);
            e.Property(up => up.Amount).HasColumnType("decimal(18,8)");
            e.HasOne(up => up.User)
                .WithMany(u => u.Portfolios)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(up => up.Asset)
                .WithMany()
                .HasForeignKey(up => up.AssetId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CurrencyPrice>(e =>
        {
            e.HasKey(cp => cp.AssetId);
            e.Property(cp => cp.CurrentPrice).HasColumnType("decimal(18,8)");
            e.HasOne(cp => cp.Asset)
                .WithMany()
                .HasForeignKey(cp => cp.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(rt => rt.Id);
            // Token kolonu unique: aynı token değeri iki kez DB'ye giremez.
            // Aynı zamanda index görevi görür — "WHERE Token = ?" sorgusu hızlı çalışır.
            e.HasIndex(rt => rt.Token).IsUnique();
            e.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silinince refresh token'ları da silinir.
        });
    }
}
