using Application.DTOs;
using Application.Features.PayoutAccounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minisource.Common.Exceptions;
using Presentaion.Middlewares;

namespace payment.Controllers;

[Route("api/v1/me")]
public class UserPayoutAccountController : PaymentControllerBase
{
    private readonly IPayoutAccountService _svc;
    public UserPayoutAccountController(IPayoutAccountService svc) => _svc = svc;

    [HttpPost("payout-accounts")]
    public async Task<ActionResult<PayoutAccountDto>> Create([FromBody] CreatePayoutAccountRequest request, CancellationToken ct)
        => Ok(await _svc.CreateAsync(GetRequiredTenantId(), GetRequiredActorUserId(), "user", request, ct));

    [HttpGet("payout-accounts")]
    public async Task<ActionResult<List<PayoutAccountDto>>> List(CancellationToken ct)
        => Ok(await _svc.GetMyAccountsAsync(GetRequiredTenantId(), "user", GetRequiredActorUserId(), ct));

    [HttpGet("payout-accounts/{accountId:guid}")]
    public async Task<ActionResult<PayoutAccountDto>> Get(Guid accountId, CancellationToken ct)
        => Ok(await _svc.GetAsync(accountId, ct));

    [HttpPatch("payout-accounts/{accountId:guid}")]
    public async Task<ActionResult<PayoutAccountDto>> Update(Guid accountId, [FromBody] UpdatePayoutAccountRequest request, CancellationToken ct)
        => Ok(await _svc.UpdateAsync(accountId, GetRequiredTenantId(), GetRequiredActorUserId(), request, ct));

    [HttpPost("payout-accounts/{accountId:guid}/set-default")]
    public async Task<IActionResult> SetDefault(Guid accountId, CancellationToken ct)
    { await _svc.SetDefaultAsync(accountId, GetRequiredTenantId(), GetRequiredActorUserId(), ct); return Ok(); }

    [HttpDelete("payout-accounts/{accountId:guid}")]
    public async Task<IActionResult> Delete(Guid accountId, CancellationToken ct)
    { await _svc.SoftDeleteAsync(accountId, GetRequiredTenantId(), GetRequiredActorUserId(), ct); return Ok(); }
}
