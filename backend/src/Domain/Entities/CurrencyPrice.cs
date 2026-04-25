namespace PortfolioTracker.Domain.Entities;

public class CurrencyPrice
{
    public int AssetId { get; set; }
    public decimal CurrentPrice { get; set; }
    public DateTime LastUpdated { get; set; }

    public Asset Asset { get; set; } = null!;
}
