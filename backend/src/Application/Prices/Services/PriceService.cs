using PortfolioTracker.Application.Common.Exceptions;
using PortfolioTracker.Application.Prices.DTOs;
using PortfolioTracker.Domain.Services;
using PortfolioTracker.Domain.ValueObjects;

namespace PortfolioTracker.Application.Prices.Services;

public class PriceService : IPriceService
{
    private readonly IExchangeRateProvider _exchangeRateProvider;
    private readonly ICurrencyConverter _currencyConverter;

    public PriceService(
        IExchangeRateProvider exchangeRateProvider,
        ICurrencyConverter currencyConverter)
    {
        _exchangeRateProvider = exchangeRateProvider ?? throw new ArgumentNullException(nameof(exchangeRateProvider));
        _currencyConverter = currencyConverter ?? throw new ArgumentNullException(nameof(currencyConverter));
    }

    public async Task<PriceResponseDto> GetPricesAsync(string baseCurrency, CancellationToken cancellationToken = default)
    {
        // Validate base currency
        CurrencyCode baseCurrencyCode;
        try
        {
            baseCurrencyCode = CurrencyCode.From(baseCurrency);
        }
        catch (ArgumentException ex)
        {
            throw new UnsupportedCurrencyException(baseCurrency, ex);
        }

        try
        {
            // Get all exchange rates from provider
            var exchangeRates = await _exchangeRateProvider.GetLatestExchangeRatesAsync(cancellationToken);

            if (exchangeRates.Count == 0)
            {
                throw new ExternalServiceException("No exchange rates available from the provider.");
            }

            // Get precious metal prices
            var preciousMetalPrices = await _exchangeRateProvider.GetLatestPreciousMetalPricesAsync(cancellationToken);

            // Convert all currency rates to base currency
            var currencyDtos = new List<CurrencyPriceDto>();
            var tryCode = CurrencyCode.TRY;

            foreach (var rate in exchangeRates)
            {
                decimal buyingPrice;
                decimal sellingPrice;

                // If base currency is the same as the rate currency, use 1:1
                if (rate.CurrencyCode == baseCurrencyCode)
                {
                    buyingPrice = 1m;
                    sellingPrice = 1m;
                }
                // If base currency is TRY, use direct rate from TCMB
                else if (baseCurrencyCode == tryCode)
                {
                    buyingPrice = rate.GetEffectiveBuyingRate();
                    sellingPrice = rate.GetEffectiveSellingRate();
                }
                else
                {
                    // Cross-currency conversion
                    // Example: If base is USD, and we want EUR price in USD
                    // We need to convert: 1 EUR = X USD
                    buyingPrice = await _currencyConverter.GetExchangeRateAsync(rate.CurrencyCode, baseCurrencyCode, cancellationToken);

                    // For selling rate, we use the selling rate instead of buying
                    // This is an approximation - in reality, we'd need separate calculation
                    var sellingRateAdjustment = rate.GetEffectiveSellingRate() / rate.GetEffectiveBuyingRate();
                    sellingPrice = buyingPrice * sellingRateAdjustment;
                }

                currencyDtos.Add(new CurrencyPriceDto
                {
                    CurrencyCode = rate.CurrencyCode.Code,
                    BuyingPrice = buyingPrice,
                    SellingPrice = sellingPrice,
                    Unit = rate.Unit
                });
            }

            // Add TRY itself if it's not in the list and base currency is not TRY
            if (baseCurrencyCode != tryCode && !exchangeRates.Any(r => r.CurrencyCode == tryCode))
            {
                var tryTryRate = await _currencyConverter.GetExchangeRateAsync(tryCode, baseCurrencyCode, cancellationToken);
                currencyDtos.Add(new CurrencyPriceDto
                {
                    CurrencyCode = "TRY",
                    BuyingPrice = tryTryRate,
                    SellingPrice = tryTryRate,
                    Unit = 1
                });
            }

            // Convert precious metal prices to base currency
            var metalDtos = new List<PreciousMetalPriceDto>();
            foreach (var metalPrice in preciousMetalPrices)
            {
                var priceInTry = new Money(metalPrice.PricePerGram, tryCode);
                var convertedPrice = await _currencyConverter.ConvertAsync(priceInTry, baseCurrencyCode, cancellationToken);

                var priceInTryPerOunce = new Money(metalPrice.PricePerOunce, tryCode);
                var convertedPricePerOunce = await _currencyConverter.ConvertAsync(priceInTryPerOunce, baseCurrencyCode, cancellationToken);

                metalDtos.Add(new PreciousMetalPriceDto
                {
                    MetalType = metalPrice.MetalType.ToString(),
                    PricePerGram = convertedPrice.Amount,
                    PricePerOunce = convertedPricePerOunce.Amount
                });
            }

            return new PriceResponseDto
            {
                BaseCurrency = baseCurrencyCode.Code,
                Timestamp = DateTime.UtcNow,
                Currencies = currencyDtos,
                PreciousMetals = metalDtos
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new ExternalServiceException("Failed to retrieve or process exchange rates.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalServiceException("Failed to communicate with external service.", ex);
        }
    }
}
