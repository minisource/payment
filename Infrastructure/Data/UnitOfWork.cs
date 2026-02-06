using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;

namespace Infrastructure.Data;

/// <summary>
/// Unit of Work implementation using EF Core.
/// Manages transactions and change tracking.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly PaymentDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    public UnitOfWork(PaymentDbContext context, ILogger<UnitOfWork> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            UpdateTimestamps();
            var result = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Saved {Count} changes to database", result);
            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency conflict while saving changes");
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error while saving changes");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        _logger.LogDebug("Started database transaction {TransactionId}", _currentTransaction.TransactionId);

        return new EfCoreTransaction(_currentTransaction, () => _currentTransaction = null, _logger);
    }

    /// <inheritdoc/>
    public bool HasActiveTransaction => _currentTransaction != null;

    /// <summary>
    /// Updates timestamps for entities being saved.
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = _context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Entity<Guid> entity)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                }
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _currentTransaction?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Transaction wrapper for EF Core transactions.
/// </summary>
internal class EfCoreTransaction : ITransaction
{
    private readonly IDbContextTransaction _transaction;
    private readonly Action _onDispose;
    private readonly ILogger _logger;
    private bool _disposed;
    private bool _completed;

    public EfCoreTransaction(
        IDbContextTransaction transaction,
        Action onDispose,
        ILogger logger)
    {
        _transaction = transaction;
        _onDispose = onDispose;
        _logger = logger;
    }

    public Guid TransactionId => _transaction.TransactionId;

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_completed)
            throw new InvalidOperationException("Transaction already completed");

        try
        {
            await _transaction.CommitAsync(cancellationToken);
            _completed = true;
            _logger.LogDebug("Committed transaction {TransactionId}", _transaction.TransactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to commit transaction {TransactionId}", _transaction.TransactionId);
            throw;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_completed)
            throw new InvalidOperationException("Transaction already completed");

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
            _completed = true;
            _logger.LogDebug("Rolled back transaction {TransactionId}", _transaction.TransactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rollback transaction {TransactionId}", _transaction.TransactionId);
            throw;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (!_completed)
            {
                // Auto-rollback if not committed
                _logger.LogWarning("Transaction {TransactionId} disposed without explicit commit/rollback - rolling back",
                    _transaction.TransactionId);
                _transaction.Rollback();
            }

            _transaction.Dispose();
            _onDispose();
            _disposed = true;
        }
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
