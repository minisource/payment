using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minisource.Common.Exceptions;
using Presentaion.Middlewares;

namespace payment.Controllers;

/// <summary>
/// Controller for wallet operations.
/// </summary>
[ApiController]
[ApiResultFilter]
[Route("api/v1/wallets")]
[Authorize]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly ILogger<WalletsController> _logger;

    public WalletsController(
        IWalletService walletService,
        ILogger<WalletsController> logger)
    {
        _walletService = walletService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current user's wallet.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Wallet details.</returns>
    [HttpGet("me")]
    public async Task<ActionResult<WalletDto>> GetMine(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedException("User ID not found in token");
        }

        var wallet = await _walletService.GetOrCreateAsync(userId, cancellationToken);
        return Ok(wallet);
    }

    /// <summary>
    /// Gets a user's wallet by user ID.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Wallet details.</returns>
    [HttpGet("users/{userId}")]
    [Authorize(Policy = "WalletAdmin")]
    public async Task<ActionResult<WalletDto>> GetByUser(
        string userId,
        CancellationToken cancellationToken)
    {
        var wallet = await _walletService.GetOrCreateAsync(userId, cancellationToken);
        return Ok(wallet);
    }

    /// <summary>
    /// Credits a user's wallet.
    /// </summary>
    /// <param name="request">Credit request details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated wallet.</returns>
    [HttpPost("credit")]
    [Authorize(Policy = "WalletAdmin")]
    public async Task<ActionResult<WalletDto>> Credit(
        [FromBody] CreditWalletRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Crediting wallet for user {UserId} with amount {Amount}",
            request.UserId,
            request.Amount);

        var wallet = await _walletService.CreditAsync(
            request.UserId,
            request.Amount,
            request.Description,
            request.ReferenceId,
            request.ReferenceType,
            cancellationToken);

        return Ok(wallet);
    }

    /// <summary>
    /// Debits a user's wallet.
    /// </summary>
    /// <param name="request">Debit request details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated wallet.</returns>
    [HttpPost("debit")]
    [Authorize(Policy = "WalletAdmin")]
    public async Task<ActionResult<WalletDto>> Debit(
        [FromBody] DebitWalletRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Debiting wallet for user {UserId} with amount {Amount}",
            request.UserId,
            request.Amount);

        var wallet = await _walletService.DebitAsync(
            request.UserId,
            request.Amount,
            request.Description,
            request.ReferenceId,
            request.ReferenceType,
            cancellationToken);

        return Ok(wallet);
    }

    /// <summary>
    /// Gets paginated transactions for a user's wallet.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of transactions.</returns>
    [HttpGet("users/{userId}/transactions")]
    [Authorize(Policy = "WalletAdmin")]
    public async Task<ActionResult<PagedResult<WalletTransactionDto>>> GetTransactions(
        string userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _walletService.GetTransactionsAsync(userId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets the current user's transactions.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of transactions.</returns>
    [HttpGet("me/transactions")]
    public async Task<ActionResult<PagedResult<WalletTransactionDto>>> GetMyTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedException("User ID not found in token");
        }

        var result = await _walletService.GetTransactionsAsync(userId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Recalculates a user's wallet balance from transaction history.
    /// Admin-only operation for fixing data inconsistencies.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated wallet with recalculated balance.</returns>
    [HttpPost("users/{userId}/recalculate")]
    [Authorize(Policy = "WalletAdmin")]
    public async Task<ActionResult<WalletDto>> RecalculateBalance(
        string userId,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Recalculating wallet balance for user {UserId}",
            userId);

        var wallet = await _walletService.RecalculateBalanceAsync(userId, cancellationToken);
        return Ok(wallet);
    }
}
