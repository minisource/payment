using Minisource.Common.Domain;

namespace Domain.Events;

public record PaymentIntentCreatedEvent(
    Guid PaymentIntentId,
    Guid TenantId,
    decimal Amount,
    string Currency,
    string Purpose) : DomainEvent;

public record PaymentIntentStartedEvent(
    Guid PaymentIntentId,
    Guid PaymentTransactionId,
    Guid TenantId,
    string ProviderCode) : DomainEvent;

public record PaymentIntentSucceededEvent(
    Guid PaymentIntentId,
    Guid TenantId,
    decimal Amount,
    string Currency) : DomainEvent;

public record PaymentIntentFailedEvent(
    Guid PaymentIntentId,
    Guid TenantId,
    decimal Amount,
    string Currency) : DomainEvent;

public record PaymentIntentCancelledEvent(
    Guid PaymentIntentId,
    Guid TenantId,
    string Reason) : DomainEvent;

public record GatewayTransactionCreatedEvent(
    Guid PaymentTransactionId,
    Guid PaymentIntentId,
    Guid TenantId,
    string ProviderCode) : DomainEvent;

public record GatewayTransactionVerifiedEvent(
    Guid PaymentTransactionId,
    Guid PaymentIntentId,
    Guid TenantId,
    decimal Amount,
    string Currency,
    string ProviderCode) : DomainEvent;

public record GatewayTransactionFailedEvent(
    Guid PaymentTransactionId,
    Guid PaymentIntentId,
    Guid TenantId,
    string? ResponseCode) : DomainEvent;

public record PaymentWalletPostedEvent(
    Guid PaymentIntentId,
    Guid PaymentTransactionId,
    Guid TenantId,
    Guid WalletTransactionId,
    decimal Amount,
    string Currency,
    string WalletBehavior) : DomainEvent;

public record GatewayConfigUpdatedEvent(
    Guid GatewayConfigId,
    string ProviderCode) : DomainEvent;
