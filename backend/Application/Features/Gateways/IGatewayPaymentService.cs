using Application.DTOs;

namespace Application.Features.Gateways;

/// <summary>
/// Service for initiating payments through gateway adapters.
/// </summary>
public interface IGatewayPaymentService
{
    Task<StartPaymentIntentResponse> StartPaymentAsync(Guid tenantId, Guid paymentIntentId, StartPaymentIntentRequest request, string? applicationCode, CancellationToken ct);
}
