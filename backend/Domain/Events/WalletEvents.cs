using Domain.Enums;
using Minisource.Common.Domain;

namespace Domain.Events;

/// <summary>
/// Event raised when a new wallet is created.
/// </summary>
public record WalletCreatedEvent(
    Guid WalletId,
    string UserId,
    DateTime CreatedAt
) : DomainEvent;

/// <summary>
/// Event raised when wallet is credited.
/// </summary>
public record WalletCreditedEvent(
    Guid WalletId,
    string UserId,
    Guid TransactionId,
    decimal Amount,
    decimal NewBalance,
    string Description,
    DateTime CreditedAt
) : DomainEvent;

/// <summary>
/// Event raised when wallet is debited.
/// </summary>
public record WalletDebitedEvent(
    Guid WalletId,
    string UserId,
    Guid TransactionId,
    decimal Amount,
    decimal NewBalance,
    string Description,
    DateTime DebitedAt
) : DomainEvent;

/// <summary>
/// Event raised when wallet balance is recalculated.
/// </summary>
public record WalletBalanceRecalculatedEvent(
    Guid WalletId,
    decimal OldBalance,
    decimal NewBalance,
    DateTime RecalculatedAt
) : DomainEvent;

/// <summary>
/// Event raised when debit operation fails due to insufficient funds.
/// </summary>
public record InsufficientFundsEvent(
    Guid WalletId,
    string UserId,
    decimal RequestedAmount,
    decimal AvailableBalance,
    DateTime OccurredAt
) : DomainEvent;
