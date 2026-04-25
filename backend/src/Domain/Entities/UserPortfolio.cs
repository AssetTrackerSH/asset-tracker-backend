namespace PortfolioTracker.Domain.Entities;

public class UserPortfolio
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int AssetId { get; set; }
    public decimal Amount { get; set; }

    public User User { get; set; } = null!;
    public Asset Asset { get; set; } = null!;
}
