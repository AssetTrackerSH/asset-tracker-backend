using PortfolioTracker.Application.Prices.DTOs;

namespace PortfolioTracker.Application.Prices.Services;

public interface IPriceService
{
    Task<PriceResponseDto> GetPricesAsync(string baseCurrency, CancellationToken cancellationToken = default);
}
