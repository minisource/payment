using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;
using Minisource.Common.Exceptions;
using Minisource.Common.Locking;

namespace Application.Services;

/// <summary>
/// Wallet service implementing wallet operations with distributed locking and transactions.
/// </summary>
public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedLockService _lockService;
    private readonly ILogger<WalletService> _logger;

    public WalletService(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        IDistributedLockService lockService,
        ILogger<WalletService> logger)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _lockService = lockService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<WalletDto> GetOrCreateAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ValidationException("UserId", "User ID is required");
        }

        var wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);

        if (wallet != null)
        {
            return MapToDto(wallet);
        }

        // Acquire lock to prevent race condition on wallet creation
        await using var walletLock = await _lockService.AcquireAsync(
            $"wallet:create:{userId}",
            TimeSpan.FromSeconds(10),
            cancellationToken);

        if (!walletLock.IsAcquired)
        {
            throw new ConflictException("Unable to acquire lock for wallet creation");
        }

        // Double-check after acquiring lock
        wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);
        if (wallet != null)
        {
            return MapToDto(wallet);
        }

        // Create new wallet
        wallet = Wallet.Create(userId, "IRR");
        await _walletRepository.AddAsync(wallet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created new wallet for user {UserId}", userId);

        return MapToDto(wallet);
    }

    /// <inheritdoc/>
    public async Task<WalletDto> GetBalanceAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ValidationException("UserId", "User ID is required");
        }

        var wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);
        if (wallet == null)
        {
            throw new NotFoundException("Wallet", userId);
        }

        return MapToDto(wallet);
    }

    /// <inheritdoc/>
    public async Task<WalletDto> CreditAsync(
        string userId,
        decimal amount,
        string description,
        string? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            throw new ValidationException("Amount", "Amount must be greater than zero");
        }

        // Acquire lock to prevent concurrent credit operations
        await using var walletLock = await _lockService.AcquireAsync(
            $"wallet:credit:{userId}",
            TimeSpan.FromSeconds(30),
            cancellationToken);

        if (!walletLock.IsAcquired)
        {
            throw new ConflictException("Unable to acquire lock for wallet credit");
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Get wallet with pessimistic lock
            var wallet = await _walletRepository.GetByUserIdForUpdateAsync(userId, cancellationToken);
            if (wallet == null)
            {
                // Create wallet if doesn't exist
                wallet = Wallet.Create(userId, "IRR");
                await _walletRepository.AddAsync(wallet, cancellationToken);
            }

            wallet.Credit(amount, description, referenceId, referenceType);

            await _walletRepository.UpdateAsync(wallet, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Credited wallet for user {UserId} with amount {Amount}", userId, amount);

            return MapToDto(wallet);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to credit wallet for user {UserId}", userId);
            throw new BusinessException("WALLET_CREDIT_FAILED", "Failed to credit wallet", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<WalletDto> DebitAsync(
        string userId,
        decimal amount,
        string description,
        string? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            throw new ValidationException("Amount", "Amount must be greater than zero");
        }

        // Acquire lock to prevent concurrent debit operations
        await using var walletLock = await _lockService.AcquireAsync(
            $"wallet:debit:{userId}",
            TimeSpan.FromSeconds(30),
            cancellationToken);

        if (!walletLock.IsAcquired)
        {
            throw new ConflictException("Unable to acquire lock for wallet debit");
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var wallet = await _walletRepository.GetByUserIdForUpdateAsync(userId, cancellationToken);
            if (wallet == null)
            {
                throw new NotFoundException("Wallet", userId);
            }

            wallet.Debit(amount, description, referenceId, referenceType);

            await _walletRepository.UpdateAsync(wallet, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Debited wallet for user {UserId} with amount {Amount}", userId, amount);

            return MapToDto(wallet);
        }
        catch (WalletException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogWarning("Insufficient funds for user {UserId}: {Message}", userId, ex.Message);
            throw new BusinessException("INSUFFICIENT_FUNDS", ex.Message, ex);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to debit wallet for user {UserId}", userId);
            throw new BusinessException("WALLET_DEBIT_FAILED", "Failed to debit wallet", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<(decimal Applied, decimal Remaining)> ApplyCreditAsync(
        string userId,
        decimal amount,
        string description,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            return (0, amount);
        }

        // Acquire lock to prevent concurrent credit application
        await using var walletLock = await _lockService.AcquireAsync(
            $"wallet:apply:{userId}",
            TimeSpan.FromSeconds(30),
            cancellationToken);

        if (!walletLock.IsAcquired)
        {
            _logger.LogWarning("Unable to acquire lock for wallet apply, proceeding without credit");
            return (0, amount);
        }

        var wallet = await _walletRepository.GetByUserIdForUpdateAsync(userId, cancellationToken);
        if (wallet == null || wallet.Balance <= 0)
        {
            return (0, amount);
        }

        var applied = Math.Min(wallet.Balance, amount);
        if (applied <= 0)
        {
            return (0, amount);
        }

        try
        {
            wallet.Debit(applied, description, null, "PaymentCredit");
            await _walletRepository.UpdateAsync(wallet, cancellationToken);

            _logger.LogInformation("Applied {Applied} credit from wallet for user {UserId}", applied, userId);

            return (applied, amount - applied);
        }
        catch (WalletException)
        {
            // Insufficient funds, return without applying
            return (0, amount);
        }
    }

    /// <inheritdoc/>
    public async Task<PagedResult<WalletTransactionDto>> GetTransactionsAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ValidationException("UserId", "User ID is required");
        }

        var wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);
        if (wallet == null)
        {
            return new PagedResult<WalletTransactionDto>
            {
                Items = new List<WalletTransactionDto>(),
                TotalCount = 0,
                Page = page,
                PageSize = pageSize
            };
        }

        var transactions = await _transactionRepository.GetByWalletIdAsync(
            wallet.Id, page, pageSize, cancellationToken);
        var totalCount = await _transactionRepository.GetTotalCountByWalletIdAsync(wallet.Id, cancellationToken);

        return new PagedResult<WalletTransactionDto>
        {
            Items = transactions.Select(MapTransactionToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <inheritdoc/>
    public async Task<WalletDto> RecalculateBalanceAsync(string userId, CancellationToken cancellationToken = default)
    {
        // Acquire lock to prevent concurrent balance recalculation
        await using var walletLock = await _lockService.AcquireAsync(
            $"wallet:recalculate:{userId}",
            TimeSpan.FromSeconds(60),
            cancellationToken);

        if (!walletLock.IsAcquired)
        {
            throw new ConflictException("Unable to acquire lock for balance recalculation");
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var wallet = await _walletRepository.GetByUserIdForUpdateAsync(userId, cancellationToken);
            if (wallet == null)
            {
                throw new NotFoundException("Wallet", userId);
            }

            var calculatedBalance = await _walletRepository.CalculateBalanceFromTransactionsAsync(userId, cancellationToken);
            var oldBalance = wallet.Balance;

            // Use reflection or a special method to update balance directly
            // For now, we'll update via the repository
            await _walletRepository.UpdateBalanceAsync(wallet.Id, calculatedBalance, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Recalculated balance for user {UserId}: {OldBalance} -> {NewBalance}",
                userId, oldBalance, calculatedBalance);

            wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);
            return MapToDto(wallet!);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to recalculate balance for user {UserId}", userId);
            throw new BusinessException("RECALCULATION_FAILED", "Failed to recalculate wallet balance", ex);
        }
    }

    private static WalletDto MapToDto(Wallet wallet) => new()
    {
        Id = wallet.Id,
        UserId = wallet.UserId,
        Balance = wallet.Balance,
        Currency = wallet.Currency,
        UpdatedAt = wallet.UpdatedAt
    };

    private static WalletTransactionDto MapTransactionToDto(WalletTransaction tx) => new()
    {
        Id = tx.Id,
        Amount = tx.Amount,
        Type = tx.Type,
        Description = tx.Description,
        ReferenceId = tx.ReferenceId,
        ReferenceType = tx.ReferenceType,
        CreatedAt = tx.CreatedAt
    };
}
