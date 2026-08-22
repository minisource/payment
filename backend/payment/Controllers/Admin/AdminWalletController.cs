using Application.DTOs;
using Application.Features.Wallets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minisource.Common.Exceptions;
using Minisource.Common.Response;
using Presentaion.Middlewares;

namespace payment.Controllers;

[Route("api/v1/admin")]
[Authorize(Policy = "WalletAdmin")]
public class AdminWalletController : PaymentControllerBase
{
    private readonly IAdminWalletService _service;
    private readonly ILogger<AdminWalletController> _logger;

    public AdminWalletController(IAdminWalletService service, ILogger<AdminWalletController> logger)
    {
        _service = service; _logger = logger;
    }

    /// <summary>Admin credit to a wallet.</summary>
    [HttpPost("wallets/{walletId:guid}/credit")]
    public async Task<ActionResult<AdminAdjustmentResponse>> Credit(
        Guid walletId, [FromBody] AdminCreditWalletRequest request, CancellationToken ct)
    {
        var result = await _service.CreditWalletAsync(GetRequiredTenantId(), walletId, GetRequiredActorUserId(), request, IdempotencyKey, ct);
        return Ok(result);
    }

    /// <summary>Admin debit from a wallet.</summary>
    [HttpPost("wallets/{walletId:guid}/debit")]
    public async Task<ActionResult<AdminAdjustmentResponse>> Debit(
        Guid walletId, [FromBody] AdminDebitWalletRequest request, CancellationToken ct)
    {
        var result = await _service.DebitWalletAsync(GetRequiredTenantId(), walletId, GetRequiredActorUserId(), request, IdempotencyKey, ct);
        return Ok(result);
    }

    /// <summary>List tenant wallets (admin).</summary>
    [HttpGet("wallets")]
    public async Task<ActionResult<List<WalletAccountDto>>> ListWallets(
        [FromQuery] string? ownerType, [FromQuery] Guid? ownerId,
        [FromQuery] string? currency, [FromQuery] string? status,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var result = await _service.GetAdminWalletsAsync(GetRequiredTenantId(), ownerType, ownerId, currency, status, skip, take, ct);
        return Ok(result);
    }

    /// <summary>Get a specific wallet.</summary>
    [HttpGet("wallets/{walletId:guid}")]
    public async Task<ActionResult<WalletAccountDto>> GetWallet(Guid walletId, CancellationToken ct)
    {
        return Ok(await _service.GetWalletAsync(walletId, ct));
    }

    /// <summary>Get wallet ledger entries.</summary>
    [HttpGet("wallets/{walletId:guid}/ledger")]
    public async Task<ActionResult<PagedResponse<LedgerEntryDto>>> GetLedger(
        Guid walletId, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        return Ok(await _service.GetLedgerAsync(walletId, skip, take, ct));
    }

    /// <summary>List tenant wallet transactions.</summary>
    [HttpGet("wallet-transactions")]
    public async Task<ActionResult<PagedResponse<WalletTransactionRecordDto>>> GetTransactions(
        [FromQuery] string? transactionType, [FromQuery] Guid? walletId,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        return Ok(await _service.GetTransactionsAsync(GetRequiredTenantId(), transactionType, walletId, skip, take, ct));
    }

    /// <summary>Get a specific transaction.</summary>
    [HttpGet("wallet-transactions/{transactionId:guid}")]
    public async Task<ActionResult<WalletTransactionRecordDto>> GetTransaction(Guid transactionId, CancellationToken ct)
    {
        return Ok(await _service.GetTransactionAsync(transactionId, ct));
    }

    /// <summary>List all ledger entries (optionally filtered by walletId, tenantId, entryType, direction, currency, date range).</summary>
    [HttpGet("ledger")]
    public async Task<ActionResult<PagedResponse<LedgerEntryDto>>> ListAllLedger(
        [FromQuery] Guid? walletId,
        [FromQuery] string? entryType,
        [FromQuery] string? direction,
        [FromQuery] string? currency,
        [FromQuery] decimal? amountMin,
        [FromQuery] decimal? amountMax,
        [FromQuery] string? referenceType,
        [FromQuery] string? referenceId,
        [FromQuery] string? dateFrom,
        [FromQuery] string? dateTo,
        [FromQuery] string? query,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 25,
        CancellationToken ct = default)
    {
        return Ok(await _service.ListAllLedgerAsync(GetEffectiveTenantId(), walletId, entryType, direction, currency,
            amountMin, amountMax, referenceType, referenceId, dateFrom, dateTo, query, skip, take, ct));
    }
}
