using Application.Features.Wallets;
using Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentaion.Middlewares;

namespace payment.Controllers;

[Route("api/v1/admin/diagnostics")]
[Authorize(Policy = "WalletAdmin")]
public class DiagnosticsController : PaymentControllerBase
{
    private readonly ILedgerHashService _ledgerHashSvc;
    private readonly IWalletConsistencyService _consistencySvc;
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(ILedgerHashService ledgerHashSvc, IWalletConsistencyService consistencySvc, ILogger<DiagnosticsController> logger)
    { _ledgerHashSvc = ledgerHashSvc; _consistencySvc = consistencySvc; _logger = logger; }

    [HttpPost("ledger/{walletId:guid}/verify-hash-chain")]
    public async Task<IActionResult> VerifyHashChain(Guid walletId, CancellationToken ct)
    {
        var (valid, errors) = await _ledgerHashSvc.VerifyWalletHashChainAsync(walletId, ct);
        return Ok(new { walletId, valid, errors });
    }

    [HttpPost("ledger/{walletId:guid}/backfill-hash-chain")]
    public async Task<IActionResult> BackfillHashChain(Guid walletId, CancellationToken ct)
    {
        var count = await _ledgerHashSvc.BackfillWalletHashChainAsync(walletId, ct);
        return Ok(new { walletId, backfilled = count });
    }

    [HttpPost("wallets/{walletId:guid}/deep-check")]
    public async Task<IActionResult> DeepCheckWallet(Guid walletId, CancellationToken ct)
    {
        var result = await _consistencySvc.CheckWalletAsync(walletId, ct);
        return Ok(result);
    }

    [HttpPost("wallets/deep-check-tenant")]
    public async Task<IActionResult> DeepCheckTenant([FromQuery] Guid? tenantId, CancellationToken ct)
    {
        var effectiveTenant = tenantId ?? (TenantId != Guid.Empty ? TenantId : throw new UnauthorizedAccessException());
        var result = await _consistencySvc.CheckTenantWalletsAsync(effectiveTenant, ct);
        return Ok(result);
    }
}
