using Application.DTOs;
using Application.Features.Withdrawals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentaion.Middlewares;

namespace payment.Controllers;

[Route("api/v1/admin/reports")]
[Authorize(Policy = "WalletAdmin")]
public class WithdrawalReportsController : PaymentControllerBase
{
    private readonly IWithdrawalService _svc;

    public WithdrawalReportsController(IWithdrawalService svc) => _svc = svc;

    [HttpGet("withdrawals/summary")]
    public async Task<ActionResult<WithdrawalSummaryDto>> Summary([FromQuery] Guid? tenantId, CancellationToken ct)
    {
        var effectiveTenantId = tenantId ?? (TenantId != Guid.Empty ? TenantId : null);
        return Ok(await _svc.GetSummaryAsync(effectiveTenantId, ct));
    }

    [HttpGet("withdrawals/pending")]
    public async Task<ActionResult<List<WithdrawalRequestDto>>> Pending(
        [FromQuery] Guid? tenantId, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var effectiveTenantId = tenantId ?? (TenantId != Guid.Empty ? TenantId : null);
        return Ok(await _svc.GetPendingAsync(effectiveTenantId, skip, take, ct));
    }
}
