namespace PortfolioTracker.Domain.Entities;

public enum TransactionType { Buy, Sell }

public class Transaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int AssetId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal Price { get; set; }
    public decimal TotalValue { get; set; }
    public DateTime Date { get; set; }

    public User User { get; set; } = null!;
    public Asset Asset { get; set; } = null!;
}
