using Domain.Entities;

namespace Application.Features.Reconciliation;

/// <summary>
/// Service for financial reconciliation operations (wallet balance, gateway payments, withdrawals).
/// </summary>
public interface IReconciliationService
{
    Task<FinancialReconciliationBatch> ReconcileWalletBalancesAsync(Guid tenantId, Guid? triggeredByUserId, CancellationToken ct);
    Task<FinancialReconciliationBatch> ReconcileGatewayPaymentsAsync(Guid tenantId, Guid? triggeredByUserId, CancellationToken ct);
    Task<FinancialReconciliationBatch> ReconcileWithdrawalPayoutsAsync(Guid tenantId, Guid? triggeredByUserId, CancellationToken ct);
    Task<FinancialReconciliationBatch> ReconcilePaymentWalletPostingAsync(Guid tenantId, Guid? triggeredByUserId, CancellationToken ct);
}
