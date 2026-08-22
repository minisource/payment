using Application.DTOs;
using Application.Features.Withdrawals;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentaion.Middlewares;

namespace payment.Controllers;

[Route("api/v1/admin")]
[Authorize(Policy = "WalletAdmin")]
public class AdminWithdrawalController : PaymentControllerBase
{
    private readonly IWithdrawalService _svc;

    public AdminWithdrawalController(IWithdrawalService svc) => _svc = svc;

    [HttpGet("withdrawal-requests")]
    public async Task<ActionResult<List<WithdrawalRequestDto>>> List(
        [FromQuery] Guid? tenantId, [FromQuery] string? status,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var effectiveTenantId = tenantId ?? (TenantId != Guid.Empty ? TenantId : null);
        var statusEnum = status != null ? Enum.TryParse<WithdrawalStatus>(status, true, out var s) ? s : (WithdrawalStatus?)null : null;
        return Ok(await _svc.AdminListAsync(effectiveTenantId, statusEnum, skip, take, ct));
    }

    [HttpGet("withdrawal-requests/{withdrawalId:guid}")]
    public async Task<ActionResult<WithdrawalRequestDto>> Get(Guid withdrawalId, CancellationToken ct)
        => Ok(await _svc.AdminGetAsync(withdrawalId, ct));

    [HttpPost("withdrawal-requests/{withdrawalId:guid}/approve")]
    public async Task<ActionResult<WithdrawalRequestDto>> Approve(Guid withdrawalId, [FromBody] AdminWithdrawalApproveRequest request, CancellationToken ct)
        => Ok(await _svc.ApproveAsync(withdrawalId, ActorUserId, request.Note, ct));

    [HttpPost("withdrawal-requests/{withdrawalId:guid}/reject")]
    public async Task<ActionResult<WithdrawalRequestDto>> Reject(Guid withdrawalId, [FromBody] AdminWithdrawalRejectRequest request, CancellationToken ct)
        => Ok(await _svc.RejectAsync(withdrawalId, ActorUserId, request.Reason, ct));

    [HttpPost("withdrawal-requests/{withdrawalId:guid}/request-more-info")]
    public async Task<ActionResult<WithdrawalRequestDto>> RequestMoreInfo(Guid withdrawalId, [FromBody] AdminWithdrawalMoreInfoRequest request, CancellationToken ct)
        => Ok(await _svc.RequestMoreInfoAsync(withdrawalId, ActorUserId, request.Reason, ct));

    [HttpPost("withdrawal-requests/{withdrawalId:guid}/mark-processing")]
    public async Task<ActionResult<WithdrawalRequestDto>> MarkProcessing(Guid withdrawalId, [FromBody] AdminWithdrawalMarkProcessingRequest request, CancellationToken ct)
        => Ok(await _svc.MarkProcessingAsync(withdrawalId, ActorUserId, request.Note, ct));

    [HttpPost("withdrawal-requests/{withdrawalId:guid}/mark-paid")]
    public async Task<ActionResult<WithdrawalRequestDto>> MarkPaid(Guid withdrawalId, [FromBody] AdminWithdrawalMarkPaidRequest request, CancellationToken ct)
        => Ok(await _svc.MarkPaidAsync(withdrawalId, ActorUserId, request, ct));

    [HttpPost("withdrawal-requests/{withdrawalId:guid}/mark-failed")]
    public async Task<ActionResult<WithdrawalRequestDto>> MarkFailed(Guid withdrawalId, [FromBody] AdminWithdrawalMarkFailedRequest request, CancellationToken ct)
        => Ok(await _svc.MarkFailedAsync(withdrawalId, ActorUserId, request, ct));
}
