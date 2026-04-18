using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Application.Common.Exceptions;
using PortfolioTracker.Application.Prices.DTOs;
using PortfolioTracker.Application.Prices.Services;

namespace PortfolioTracker.Api.Endpoints;

public static class PricesEndpoints
{
    public static IEndpointRouteBuilder MapPricesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/prices")
            .WithTags("Prices");

        group.MapGet("", GetPrices)
            .WithName("GetPrices")
            .WithSummary("Get currency and precious metal prices in specified base currency")
            .WithDescription("Returns current exchange rates and precious metal prices converted to the requested base currency. Data is sourced from TCMB (Turkish Central Bank) and cached for 10 minutes.")
            .Produces<PriceResponseDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status502BadGateway)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static async Task<IResult> GetPrices(
        [FromQuery] string? baseCurrency,
        IPriceService priceService,
        CancellationToken cancellationToken)
    {
        try
        {
            // Default to TRY if not specified
            baseCurrency ??= "TRY";

            var result = await priceService.GetPricesAsync(baseCurrency, cancellationToken);
            return Results.Ok(result);
        }
        catch (UnsupportedCurrencyException ex)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Unsupported Currency",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            });
        }
        catch (ExternalServiceException ex)
        {
            return Results.Problem(
                title: "External Service Error",
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway,
                type: "https://tools.ietf.org/html/rfc7231#section-6.6.3");
        }
        catch (Exception)
        {
            // Log the exception (logger would be injected in a real scenario)
            return Results.Problem(
                title: "Internal Server Error",
                detail: "An unexpected error occurred while processing your request.",
                statusCode: StatusCodes.Status500InternalServerError,
                type: "https://tools.ietf.org/html/rfc7231#section-6.6.1");
        }
    }
}
