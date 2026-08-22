using Application.DTOs;
using Application.Features.Withdrawals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minisource.Common.Exceptions;
using Presentaion.Middlewares;

namespace payment.Controllers;

[Route("api/v1/me")]
public class UserWithdrawalController : PaymentControllerBase
{
    private readonly IWithdrawalService _svc;
    public UserWithdrawalController(IWithdrawalService svc) => _svc = svc;

    [HttpPost("withdrawal-requests")]
    public async Task<ActionResult<WithdrawalRequestDto>> Create([FromBody] CreateWithdrawalRequest request, CancellationToken ct)
        => Ok(await _svc.CreateAsync(GetRequiredTenantId(), GetRequiredActorUserId(), "user", request, ApplicationCode, IdempotencyKey, ct));

    [HttpGet("withdrawal-requests")]
    public async Task<ActionResult<List<WithdrawalRequestDto>>> List([FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
        => Ok(await _svc.GetMyRequestsAsync(GetRequiredTenantId(), "user", GetRequiredActorUserId(), skip, take, ct));

    [HttpGet("withdrawal-requests/{withdrawalId:guid}")]
    public async Task<ActionResult<WithdrawalRequestDto>> Get(Guid withdrawalId, CancellationToken ct)
        => Ok(await _svc.GetAsync(withdrawalId, ct));

    [HttpPost("withdrawal-requests/{withdrawalId:guid}/cancel")]
    public async Task<ActionResult<WithdrawalRequestDto>> Cancel(Guid withdrawalId, [FromBody] CancelWithdrawalRequest request, CancellationToken ct)
        => Ok(await _svc.CancelAsync(withdrawalId, GetRequiredTenantId(), GetRequiredActorUserId(), request.Reason, ct));
}
