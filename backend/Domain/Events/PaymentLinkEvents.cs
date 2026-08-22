using Minisource.Common.Domain;

namespace Domain.Events;

public record PaymentLinkCreatedEvent(
    Guid PaymentLinkId,
    Guid TenantId,
    Guid RecipientWalletId,
    string Title,
    DateTime CreatedAt) : DomainEvent;

public record PaymentLinkUpdatedEvent(
    Guid PaymentLinkId,
    Guid TenantId) : DomainEvent;

public record PaymentLinkPaymentStartedEvent(
    Guid PaymentLinkId,
    Guid PaymentIntentId,
    Guid TenantId,
    decimal Amount,
    string Currency) : DomainEvent;

public record PaymentLinkPaymentSucceededEvent(
    Guid PaymentLinkId,
    Guid PaymentIntentId,
    Guid PaymentTransactionId,
    Guid TenantId,
    decimal Amount,
    string Currency) : DomainEvent;

public record PaymentLinkPaymentFailedEvent(
    Guid PaymentLinkId,
    Guid PaymentIntentId,
    Guid TenantId,
    decimal Amount,
    string Currency) : DomainEvent;
