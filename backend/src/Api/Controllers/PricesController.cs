using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Application.Common.Exceptions;
using PortfolioTracker.Application.Prices.DTOs;
using PortfolioTracker.Application.Prices.Services;

namespace PortfolioTracker.Api.Controllers;

[ApiController]
[Route("api/prices")]
[Produces("application/json")]
public class PricesController : ControllerBase
{
    private readonly IPriceService _priceService;

    public PricesController(IPriceService priceService)
    {
        _priceService = priceService;
    }

    /// <summary>Döviz ve kıymetli maden fiyatlarını döner</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PriceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetPrices(
        [FromQuery] string? baseCurrency,
        CancellationToken cancellationToken)
    {
        try
        {
            baseCurrency ??= "TRY";
            var result = await _priceService.GetPricesAsync(baseCurrency, cancellationToken);
            return Ok(result);
        }
        catch (UnsupportedCurrencyException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unsupported Currency",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (ExternalServiceException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
            {
                Title = "External Service Error",
                Detail = ex.Message,
                Status = StatusCodes.Status502BadGateway
            });
        }
    }
}
