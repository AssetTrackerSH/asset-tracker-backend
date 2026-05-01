using PortfolioTracker.Domain.Enums;

namespace PortfolioTracker.Domain.Entities;

public class Asset
{
    public int Id { get; private set; }
    public string Symbol { get; private set; }
    public string Name { get; private set; }
    public AssetType Type { get; private set; }

    private Asset() { Symbol = string.Empty; Name = string.Empty; }

    public Asset(string symbol, string name, AssetType type)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Symbol = symbol.ToUpperInvariant();
        Name = name;
        Type = type;
    }
}
