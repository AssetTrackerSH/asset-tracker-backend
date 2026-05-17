using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Application.Portfolio.DTOs;
using PortfolioTracker.Application.Portfolio.Services;

namespace PortfolioTracker.Api.Controllers;

[ApiController]
[Route("api/assets")]
[Produces("application/json")]
public class AssetsController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;

    public AssetsController(IPortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
    }

    /// <summary>Desteklenen tüm varlıkları döner</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AssetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssets()
    {
        var assets = await _portfolioService.GetAssetsAsync();
        return Ok(assets);
    }
}
