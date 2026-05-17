using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Application.Portfolio.DTOs;
using PortfolioTracker.Application.Portfolio.Services;

namespace PortfolioTracker.Api.Controllers;

[ApiController]
[Route("api/portfolio")]
[Authorize]
[Produces("application/json")]
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;

    public PortfolioController(IPortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
    }

    /// <summary>Portföy özeti — toplam maliyet, net kazanç, getiri %</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(PortfolioSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary()
    {
        var userId = GetUserId();
        var summary = await _portfolioService.GetSummaryAsync(userId);
        return Ok(summary);
    }

    /// <summary>Kullanıcının portföyünü döner</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PortfolioItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPortfolio()
    {
        var userId = GetUserId();
        var items = await _portfolioService.GetPortfolioAsync(userId);
        return Ok(items);
    }

    /// <summary>Portföye yeni varlık ekle</summary>
    [HttpPost("assets")]
    [ProducesResponseType(typeof(PortfolioItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAsset([FromBody] AddAssetRequest request)
    {
        try
        {
            var userId = GetUserId();
            var item = await _portfolioService.AddAssetAsync(userId, request);
            return CreatedAtAction(nameof(GetPortfolio), item);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Bulunamadı", Detail = ex.Message, Status = 404 });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails { Title = "Çakışma", Detail = ex.Message, Status = 409 });
        }
    }

    /// <summary>Varlık miktarını güncelle</summary>
    [HttpPut("assets/{id:guid}")]
    [ProducesResponseType(typeof(PortfolioItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsset(Guid id, [FromBody] UpdateAssetRequest request)
    {
        try
        {
            var userId = GetUserId();
            var item = await _portfolioService.UpdateAssetAsync(userId, id, request);
            return Ok(item);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Bulunamadı", Detail = ex.Message, Status = 404 });
        }
    }

    /// <summary>Mevcut varlığa ek alım yap (ağırlıklı ortalama maliyet hesaplanır)</summary>
    [HttpPost("assets/{id:guid}/buy")]
    [ProducesResponseType(typeof(PortfolioItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuyMore(Guid id, [FromBody] BuyAssetRequest request)
    {
        try
        {
            var userId = GetUserId();
            var item = await _portfolioService.BuyMoreAsync(userId, id, request);
            return Ok(item);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Bulunamadı", Detail = ex.Message, Status = 404 });
        }
    }

    /// <summary>Varlığın işlem geçmişini döner</summary>
    [HttpGet("assets/{id:guid}/transactions")]
    [ProducesResponseType(typeof(IEnumerable<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var transactions = await _portfolioService.GetTransactionsAsync(userId, id);
            return Ok(transactions);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Bulunamadı", Detail = ex.Message, Status = 404 });
        }
    }

    /// <summary>Varlık sat (kısmi veya tam)</summary>
    [HttpPost("assets/{id:guid}/sell")]
    [ProducesResponseType(typeof(SellResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SellAsset(Guid id, [FromBody] SellAssetRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _portfolioService.SellAssetAsync(userId, id, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Bulunamadı", Detail = ex.Message, Status = 404 });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Geçersiz İstek", Detail = ex.Message, Status = 400 });
        }
    }

    /// <summary>Portföyden varlık sil</summary>
    [HttpDelete("assets/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsset(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _portfolioService.DeleteAssetAsync(userId, id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Bulunamadı", Detail = ex.Message, Status = 404 });
        }
    }

    // JWT token'dan UserId'yi çıkarır.
    // "sub" claim'i JwtTokenService tarafından UserId olarak set edildi.
    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Token geçersiz.");
        return Guid.Parse(sub);
    }
}
