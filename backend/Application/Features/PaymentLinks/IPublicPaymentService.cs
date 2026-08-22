using Application.DTOs;

namespace Application.Features.PaymentLinks;

/// <summary>
/// Service for public-facing payment operations (no auth required).
/// </summary>
public interface IPublicPaymentService
{
    Task<PublicPaymentLinkResponse> GetPublicLinkAsync(string token, CancellationToken ct);
    Task<PublicPayResponse> StartPublicPaymentAsync(string token, PublicPayRequest request, string? applicationCode, string? ipAddress, CancellationToken ct);
}
