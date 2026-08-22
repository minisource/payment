using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Minisource.Common.Exceptions;

namespace Application.Features.Payments;

/// <summary>
/// Service for querying payments (read operations only).
/// </summary>
public class PaymentQueryService : IPaymentQueryService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PaymentQueryService(
        IPaymentRepository paymentRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _paymentRepository = paymentRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public async Task<PaymentDto> GetPaymentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment == null)
        {
            throw new NotFoundException("Payment", id);
        }

        // Check access
        var currentUserId = GetCurrentUserId();
        if (!string.IsNullOrEmpty(currentUserId) && payment.UserId != currentUserId && !IsAdmin())
        {
            throw new ForbiddenException("You don't have access to this payment");
        }

        return MapToDto(payment);
    }

    /// <inheritdoc/>
    public async Task<PagedResult<PaymentDto>> GetPaymentsAsync(
        string? status,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var items = await _paymentRepository.GetPaymentsAsync(userId, status, from, to, page, pageSize, cancellationToken);
        var totalCount = await _paymentRepository.GetTotalCountAsync(userId, status, from, to, cancellationToken);

        return new PagedResult<PaymentDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <inheritdoc/>
    public async Task<List<PaymentLogDto>> GetPaymentLogsAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdWithLogsAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            throw new NotFoundException("Payment", paymentId);
        }

        return payment.Logs.Select(l => new PaymentLogDto
        {
            Id = l.Id,
            PaymentId = l.PaymentId,
            Action = l.Action,
            Details = l.Details ?? string.Empty,
            Timestamp = l.CreatedAt
        }).ToList();
    }

    private string? GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
    }

    private bool IsAdmin()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return false;

        return user.HasClaim("permission", "PAYMENT_MANAGE") ||
               user.HasClaim("permission", "WALLET_MANAGE") ||
               user.HasClaim("role", "admin");
    }

    private static PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            TrackingNumber = payment.TrackingNumber,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Status = payment.Status,
            Gateway = payment.Gateway,
            CallbackUrl = payment.CallbackUrl,
            ReturnUrl = payment.ReturnUrl,
            Metadata = payment.Metadata,
            UserId = payment.UserId,
            CreditApplied = payment.CreditApplied,
            AmountDue = payment.AmountDue,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt
        };
    }
}
