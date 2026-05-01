using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Domain.Entities;

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
