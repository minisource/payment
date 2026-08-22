using Application.DTOs;
using Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentaion.Middlewares;

namespace payment.Controllers;

[Route("api/v1/admin/reports")]
[Authorize(Policy = "WalletAdmin")]
public class WalletReportsController : PaymentControllerBase
{
    private readonly IWalletAccountRepository _walletRepo;

    public WalletReportsController(IWalletAccountRepository walletRepo) => _walletRepo = walletRepo;

    /// <summary>Get wallet liability report grouped by currency.</summary>
    [HttpGet("wallets/liabilities")]
    public async Task<ActionResult<TenantLiabilityDto>> GetLiabilities(CancellationToken ct)
    {
        var groups = await _walletRepo.GetTenantLiabilityAsync(TenantId, ct);
        var items = groups.Select(g => new TenantLiabilityItem
        {
            Currency = g.Key,
            AvailableTotal = g.Value.available,
            LockedTotal = g.Value.locked,
            PendingTotal = g.Value.pending,
            TotalLiability = g.Value.available + g.Value.locked + g.Value.pending,
            WalletCount = g.Value.count
        }).ToList();

        return Ok(new TenantLiabilityDto { TenantId = TenantId, Items = items });
    }

    /// <summary>Get wallet summary counts.</summary>
    [HttpGet("wallets/summary")]
    public async Task<ActionResult<object>> GetSummary(CancellationToken ct)
    {
        var count = await _walletRepo.CountByTenantAsync(TenantId, ct);
        var groups = await _walletRepo.GetTenantLiabilityAsync(TenantId, ct);
        return Ok(new { TenantId, TotalWallets = count, CurrencyBreakdown = groups });
    }
}
