using Application.DTOs;
using Application.Features.PayoutAccounts;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentaion.Middlewares;

namespace payment.Controllers;

[Route("api/v1/admin")]
[Authorize(Policy = "WalletAdmin")]
public class AdminPayoutAccountController : PaymentControllerBase
{
    private readonly IPayoutAccountService _svc;

    public AdminPayoutAccountController(IPayoutAccountService svc) => _svc = svc;

    [HttpGet("payout-accounts")]
    public async Task<ActionResult<List<PayoutAccountDto>>> List([FromQuery] Guid? tenantId, [FromQuery] string? status, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var effectiveTenantId = tenantId ?? (TenantId != Guid.Empty ? TenantId : null);
        var statusEnum = status != null ? Enum.TryParse<PayoutAccountStatus>(status, true, out var s) ? s : (PayoutAccountStatus?)null : null;
        return Ok(await _svc.AdminListAsync(effectiveTenantId, statusEnum, skip, take, ct));
    }

    [HttpGet("payout-accounts/{accountId:guid}")]
    public async Task<ActionResult<PayoutAccountDto>> Get(Guid accountId, CancellationToken ct)
        => Ok(await _svc.AdminGetAsync(accountId, ct));

    [HttpPost("payout-accounts/{accountId:guid}/verify")]
    public async Task<IActionResult> Verify(Guid accountId, CancellationToken ct)
    { await _svc.AdminVerifyAsync(accountId, ActorUserId, ct); return Ok(); }

    [HttpPost("payout-accounts/{accountId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid accountId, [FromBody] AdminRejectPayoutAccountRequest request, CancellationToken ct)
    { await _svc.AdminRejectAsync(accountId, ActorUserId, request.Reason, ct); return Ok(); }

    [HttpPost("payout-accounts/{accountId:guid}/disable")]
    public async Task<IActionResult> Disable(Guid accountId, CancellationToken ct)
    { await _svc.AdminDisableAsync(accountId, ct); return Ok(); }
}
