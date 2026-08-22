using Application.DTOs;
using Application.Features.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minisource.Common.Exceptions;
using Presentaion.Middlewares;

namespace payment.Controllers;

[Route("api/v1/admin")]
[Authorize(Policy = "WalletAdmin")]
public class SettingsController : PaymentControllerBase
{
    private readonly ISettingsService _service;

    public SettingsController(ISettingsService service) => _service = service;

    /// <summary>Get global payment settings.</summary>
    [HttpGet("payment-settings")]
    public async Task<ActionResult<PaymentSettingsDto>> GetPaymentSettings(CancellationToken ct)
        => Ok(await _service.GetPaymentSettingsAsync(ct));

    /// <summary>Update global payment settings.</summary>
    [HttpPatch("payment-settings")]
    public async Task<ActionResult<PaymentSettingsDto>> UpdatePaymentSettings(
        [FromBody] UpdatePaymentSettingsRequest request, CancellationToken ct)
        => Ok(await _service.UpdatePaymentSettingsAsync(request, ct));

    /// <summary>Get tenant payment settings.</summary>
    [HttpGet("tenant-payment-settings")]
    public async Task<ActionResult<TenantPaymentSettingsDto>> GetTenantSettings(CancellationToken ct)
        => Ok(await _service.GetTenantPaymentSettingsAsync(TenantId, ct));

    /// <summary>Update tenant payment settings.</summary>
    [HttpPatch("tenant-payment-settings")]
    public async Task<ActionResult<TenantPaymentSettingsDto>> UpdateTenantSettings(
        [FromBody] UpdateTenantPaymentSettingsRequest request, CancellationToken ct)
        => Ok(await _service.UpdateTenantPaymentSettingsAsync(TenantId, request, ct));

    /// <summary>Get fee rules for tenant.</summary>
    [HttpGet("fee-rules")]
    public async Task<ActionResult<List<Domain.Entities.FeeRule>>> GetFeeRules(CancellationToken ct)
        => Ok(await _service.GetFeeRulesAsync(TenantId, ct));

    /// <summary>Get limit rules for tenant.</summary>
    [HttpGet("limit-rules")]
    public async Task<ActionResult<List<Domain.Entities.LimitRule>>> GetLimitRules(CancellationToken ct)
        => Ok(await _service.GetLimitRulesAsync(TenantId, ct));

    /// <summary>Get risk rules for tenant.</summary>
    [HttpGet("risk-rules")]
    public async Task<ActionResult<List<Domain.Entities.RiskRule>>> GetRiskRules(CancellationToken ct)
        => Ok(await _service.GetRiskRulesAsync(TenantId, ct));
}
