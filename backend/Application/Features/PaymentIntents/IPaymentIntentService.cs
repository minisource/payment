using Application.DTOs;

namespace Application.Features.PaymentIntents;

/// <summary>
/// Service for managing payment intents (create, get, list, cancel).
/// </summary>
public interface IPaymentIntentService
{
    Task<PaymentIntentDto> CreateAsync(Guid tenantId, Guid actorUserId, CreatePaymentIntentRequest request,
        string? applicationCode, string? idempotencyKey, CancellationToken ct);
    Task<PaymentIntentDto> GetAsync(Guid paymentIntentId, CancellationToken ct);
    Task<List<PaymentIntentDto>> GetUserIntentsAsync(Guid tenantId, Guid? userId, int skip, int take, CancellationToken ct);
    Task<StartPaymentIntentResponse> StartAsync(Guid tenantId, Guid paymentIntentId, StartPaymentIntentRequest request, CancellationToken ct);
    Task<PaymentIntentDto> CancelAsync(Guid tenantId, Guid paymentIntentId, string reason, CancellationToken ct);
    Task<List<PaymentTransactionDto>> GetTransactionsAsync(Guid paymentIntentId, CancellationToken ct);
}
