using Application.DTOs;
using Domain.Enums;
using Domain.Repositories;
using Minisource.Common.Exceptions;

namespace Application.Features.Refunds;

/// <summary>
/// Read-only query service for refund requests.
/// </summary>
public class RefundQueryService : IRefundQueryService
{
    private readonly IRefundRequestRepository _refundRepo;

    public RefundQueryService(IRefundRequestRepository refundRepo)
    {
        _refundRepo = refundRepo;
    }

    public async Task<RefundRequestDto?> GetByIdAsync(Guid refundId, CancellationToken ct = default)
    {
        var refund = await _refundRepo.GetByIdAsync(refundId, ct);
        return refund is null ? null : MapToDto(refund);
    }

    public async Task<(List<RefundRequestDto> Items, int Total)> ListAsync(
        Guid? tenantId,
        string? status = null,
        Guid? paymentTransactionId = null,
        Guid? walletId = null,
        string? providerCode = null,
        string? currency = null,
        DateTime? from = null,
        DateTime? to = null,
        string? query = null,
        string? applicationCode = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default)
    {
        var statusEnum = status != null
            ? Enum.Parse<RefundRequestStatus>(status, ignoreCase: true)
            : (RefundRequestStatus?)null;

        var (items, total) = await _refundRepo.ListAsync(
            tenantId, statusEnum, paymentTransactionId, walletId,
            providerCode, currency, from, to, query, applicationCode,
            skip, take, ct);

        return (items.Select(MapToDto).ToList(), total);
    }

    private static RefundRequestDto MapToDto(Domain.Entities.RefundRequest r) => new()
    {
        Id = r.Id,
        TenantId = r.TenantId,
        ApplicationCode = r.ApplicationCode,
        PaymentIntentId = r.PaymentIntentId,
        PaymentTransactionId = r.PaymentTransactionId,
        WalletId = r.WalletId,
        RequestedByUserId = r.RequestedByUserId,
        RequestedByAdminId = r.RequestedByAdminId,
        RequestSource = r.RequestSource,
        Amount = r.Amount,
        Currency = r.Currency,
        Reason = r.Reason,
        AdminNote = r.AdminNote,
        RejectReason = r.RejectReason,
        Status = r.Status.ToString(),
        GatewayConfigId = r.GatewayConfigId,
        ProviderCode = r.ProviderCode,
        GatewayName = r.GatewayName,
        GatewayRefundId = r.GatewayRefundId,
        GatewayTrackingCode = r.GatewayTrackingCode,
        GatewayReferenceId = r.GatewayReferenceId,
        WalletHoldId = r.WalletHoldId,
        ApprovedByUserId = r.ApprovedByUserId,
        ApprovedAt = r.ApprovedAt,
        ProcessedByUserId = r.ProcessedByUserId,
        ProcessedAt = r.ProcessedAt,
        CompletedAt = r.CompletedAt,
        FailedAt = r.FailedAt,
        FailureCode = r.FailureCode,
        FailureMessage = r.FailureMessage,
        IdempotencyKey = r.IdempotencyKey,
        CorrelationId = r.CorrelationId,
        RequestId = r.RequestId,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        Attempts = r.Attempts.Select(a => new RefundGatewayAttemptDto
        {
            Id = a.Id,
            AttemptNo = a.AttemptNo,
            Status = a.Status.ToString(),
            GatewayRefundId = a.GatewayRefundId,
            GatewayTrackingCode = a.GatewayTrackingCode,
            GatewayStatus = a.GatewayStatus,
            HttpStatusCode = a.HttpStatusCode,
            ErrorCode = a.ErrorCode,
            ErrorMessage = a.ErrorMessage,
            StartedAt = a.StartedAt,
            FinishedAt = a.FinishedAt,
        }).ToList()
    };
}
