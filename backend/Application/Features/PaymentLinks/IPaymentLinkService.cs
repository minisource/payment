using Application.DTOs;
using Domain.Entities;

namespace Application.Features.PaymentLinks;

/// <summary>
/// Service for managing payment links (create, get, list, update, pause, resume, delete).
/// </summary>
public interface IPaymentLinkService
{
    Task<CreatePaymentLinkResponse> CreateAsync(Guid tenantId, Guid actorUserId, CreatePaymentLinkRequest request,
        string? applicationCode, string? publicBaseUrl, CancellationToken ct);
    Task<PaymentLinkDto> GetAsync(Guid paymentLinkId, Guid? tenantId, string? publicBaseUrl, CancellationToken ct);
    Task<List<PaymentLinkDto>> GetOwnerLinksAsync(Guid tenantId, string ownerType, Guid ownerId, int skip, int take, string? publicBaseUrl, CancellationToken ct);
    Task<PaymentLinkDto> UpdateAsync(Guid paymentLinkId, Guid? tenantId, Guid actorUserId, UpdatePaymentLinkRequest request, CancellationToken ct);
    Task PauseAsync(Guid paymentLinkId, Guid? tenantId, CancellationToken ct);
    Task ResumeAsync(Guid paymentLinkId, Guid? tenantId, CancellationToken ct);
    Task DisableAsync(Guid paymentLinkId, Guid? tenantId, CancellationToken ct);
    Task SoftDeleteAsync(Guid paymentLinkId, Guid? tenantId, CancellationToken ct);
    Task<List<PaymentIntentDto>> GetPaymentsAsync(Guid paymentLinkId, CancellationToken ct);
    Task RecordSuccessfulPaymentAsync(Guid paymentLinkId, decimal amount, CancellationToken ct);
    Task<PaymentLink?> GetByTokenHashAsync(string tokenHash, CancellationToken ct);
}
