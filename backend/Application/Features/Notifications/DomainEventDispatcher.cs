using Domain.Abstractions;
using Domain.Events;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;

namespace Application.Features.Notifications;

/// <summary>
/// Routes domain events to IPaymentNotificationService for in-app notification delivery.
/// Handles both user-facing and admin-facing events.
/// Wallet events are skipped because the old Wallet entity lacks TenantId.
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPaymentNotificationService _notificationService;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(
        IPaymentNotificationService notificationService,
        ILogger<DomainEventDispatcher> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct)
    {
        foreach (var domainEvent in domainEvents)
        {
            try
            {
                await DispatchSingleAsync(domainEvent, ct);
            }
            catch (Exception ex)
            {
                // Never let notification failure break the business transaction
                _logger.LogError(ex, "Domain event dispatch failed for {EventType}", domainEvent.GetType().Name);
            }
        }
    }

    private Task DispatchSingleAsync(IDomainEvent domainEvent, CancellationToken ct)
    {
        return domainEvent switch
        {
            // ═══════════════════════════════════════════════════
            //  USER NOTIFICATIONS
            // ═══════════════════════════════════════════════════

            // PaymentIntent events don't carry UserId — user lookup from
            // PaymentIntent entity is TODO. For now, these events only
            // trigger admin notifications via the outbox path.

            // ═══════════════════════════════════════════════════
            //  ADMIN NOTIFICATIONS — Refunds
            // ═══════════════════════════════════════════════════

            RefundRequestedEvent e => SendAdmin(
                PaymentNotificationKeys.RefundRequestedAdmin,
                e.TenantId,
                "New Refund Request",
                $"Refund of {e.Amount:N0} {e.Currency} requested. Reason: {e.Reason ?? "N/A"}",
                new Dictionary<string, string>
                {
                    ["refund_request_id"] = e.RefundRequestId.ToString(),
                    ["amount"] = e.Amount.ToString(),
                    ["currency"] = e.Currency,
                    ["payment_transaction_id"] = e.PaymentTransactionId.ToString()
                },
                $"refund:{e.RefundRequestId}:requested:admin",
                ct),

            RefundCompletedEvent e => SendAdmin(
                PaymentNotificationKeys.RefundCompletedAdmin,
                e.TenantId,
                "Refund Completed",
                $"Refund of {e.Amount:N0} {e.Currency} completed successfully.",
                new Dictionary<string, string>
                {
                    ["refund_request_id"] = e.RefundRequestId.ToString(),
                    ["amount"] = e.Amount.ToString(),
                    ["currency"] = e.Currency
                },
                $"refund:{e.RefundRequestId}:completed:admin",
                ct),

            RefundManualReviewRequiredEvent e => SendAdmin(
                PaymentNotificationKeys.RefundRequiresManualReviewAdmin,
                e.TenantId,
                "Refund Requires Manual Review",
                $"Refund ({e.RefundRequestId}) needs manual review. Code: {e.FailureCode} — {e.FailureMessage}",
                new Dictionary<string, string>
                {
                    ["refund_request_id"] = e.RefundRequestId.ToString(),
                    ["failure_code"] = e.FailureCode,
                    ["failure_message"] = e.FailureMessage ?? ""
                },
                $"refund:{e.RefundRequestId}:manual_review:admin",
                ct),

            RefundRejectedEvent e => SendAdmin(
                PaymentNotificationKeys.RefundRejected,
                e.TenantId,
                "Refund Rejected",
                $"Refund ({e.RefundRequestId}) rejected: {e.RejectReason}",
                new Dictionary<string, string>
                {
                    ["refund_request_id"] = e.RefundRequestId.ToString(),
                    ["reject_reason"] = e.RejectReason ?? ""
                },
                $"refund:{e.RefundRequestId}:rejected:admin",
                ct),

            RefundFailedEvent e => SendAdmin(
                PaymentNotificationKeys.RefundFailed,
                e.TenantId,
                "Refund Failed",
                $"Refund ({e.RefundRequestId}) failed: {e.FailureCode} — {e.FailureMessage}",
                new Dictionary<string, string>
                {
                    ["refund_request_id"] = e.RefundRequestId.ToString(),
                    ["failure_code"] = e.FailureCode,
                    ["failure_message"] = e.FailureMessage ?? ""
                },
                $"refund:{e.RefundRequestId}:failed:admin",
                ct),

            RefundGatewayFailedEvent e => SendAdmin(
                PaymentNotificationKeys.GatewayCallbackFailedAdmin,
                e.TenantId,
                "Refund Gateway Failed",
                $"Gateway call failed for refund ({e.RefundRequestId}): {e.ErrorCode} — {e.ErrorMessage}",
                new Dictionary<string, string>
                {
                    ["refund_request_id"] = e.RefundRequestId.ToString(),
                    ["error_code"] = e.ErrorCode,
                    ["error_message"] = e.ErrorMessage ?? "",
                    ["requires_manual_review"] = e.RequiresManualReview.ToString()
                },
                $"refund:{e.RefundRequestId}:gateway_failed:admin",
                ct),

            // ═══════════════════════════════════════════════════
            //  UNHANDLED — logged for visibility
            // ═══════════════════════════════════════════════════

            // Wallet events (WalletCreditedEvent, WalletDebitedEvent, etc.)
            // lack TenantId — the old Wallet entity doesn't carry it.
            // TODO: migrate Wallet → WalletAccount for TenantId awareness,
            // then wire wallet notifications.

            PaymentIntentSucceededEvent or PaymentIntentFailedEvent
                => LogSkip(domainEvent, "PaymentIntent events lack UserId — user lookup from PaymentIntent entity not yet implemented"),

            _ => Task.CompletedTask
        };
    }

    private Task SendAdmin(
        string notificationKey,
        Guid tenantId,
        string title,
        string body,
        Dictionary<string, string> data,
        string idempotencyKey,
        CancellationToken ct)
    {
        return _notificationService.SendAsync(new PaymentNotificationEvent
        {
            NotificationKey = notificationKey,
            TenantId = tenantId,
            RecipientType = "admin",
            Title = title,
            Body = body,
            Data = data,
            IdempotencyKey = idempotencyKey
        }, ct);
    }

    private Task LogSkip(IDomainEvent de, string reason)
    {
        _logger.LogDebug("Skipping notification for {EventType}: {Reason}", de.GetType().Name, reason);
        return Task.CompletedTask;
    }
}
