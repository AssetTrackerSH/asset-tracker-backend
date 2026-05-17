using PortfolioTracker.Domain.Entities;

namespace PortfolioTracker.Application.Portfolio.DTOs;

public record TransactionDto(
    Guid Id,
    string Symbol,
    string Name,
    TransactionType Type,
    decimal Amount,
    decimal Price,
    decimal TotalValue,
    DateTime Date
);
