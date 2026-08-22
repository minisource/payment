using Minisource.Common.Domain;

namespace Domain.Events;

public record WalletAccountCreatedEvent(
    Guid WalletAccountId,
    Guid TenantId,
    string OwnerType,
    Guid OwnerId,
    string Currency,
    DateTime CreatedAt
) : DomainEvent;

public record WalletBalanceChangedEvent(
    Guid WalletAccountId,
    Guid TenantId,
    Guid LedgerEntryId,
    decimal AmountChanged,
    decimal NewAvailableBalance,
    string Currency,
    DateTime OccurredAt
) : DomainEvent;

public record WalletAdminCreditedEvent(
    Guid WalletAccountId,
    Guid TenantId,
    Guid WalletTransactionId,
    decimal Amount,
    string Currency,
    string Reason,
    Guid AdminUserId,
    DateTime OccurredAt
) : DomainEvent;

public record WalletAdminDebitedEvent(
    Guid WalletAccountId,
    Guid TenantId,
    Guid WalletTransactionId,
    decimal Amount,
    string Currency,
    string Reason,
    Guid AdminUserId,
    DateTime OccurredAt
) : DomainEvent;

public record PaymentSettingsUpdatedEvent(
    Guid SettingsId,
    string SettingType,
    DateTime UpdatedAt
) : DomainEvent;

public record TenantPaymentSettingsUpdatedEvent(
    Guid TenantId,
    Guid SettingsId,
    DateTime UpdatedAt
) : DomainEvent;
