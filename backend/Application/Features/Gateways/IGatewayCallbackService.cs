using Application.DTOs;

namespace Application.Features.Gateways;

/// <summary>
/// Service for handling gateway callbacks and manual payment verification.
/// </summary>
public interface IGatewayCallbackService
{
    Task<PaymentResultDto> HandleCallbackAsync(Guid paymentTransactionId, string? queryString, string? rawBody, CancellationToken ct);
    Task<PaymentTransactionDto> ManualVerifyAsync(Guid paymentTransactionId, Guid actorUserId, CancellationToken ct);
}
