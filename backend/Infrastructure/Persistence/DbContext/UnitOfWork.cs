using System.Text.Json;
using Domain.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;

namespace Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation using EF Core.
/// Manages transactions and change tracking.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly PaymentDbContext _context;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    public UnitOfWork(
        PaymentDbContext context,
        IDomainEventDispatcher domainEventDispatcher,
        ILogger<UnitOfWork> logger)
    {
        _context = context;
        _domainEventDispatcher = domainEventDispatcher;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Collect domain events and dispatch them BEFORE saving — ensures atomicity
            var domainEvents = CollectDomainEvents();

            // 1. Dispatch in-process (e.g., send in-app notifications immediately)
            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

            // 2. Create outbox events for async external publishing (webhooks, notifier, message bus)
            await CreateOutboxEventsAsync(domainEvents, cancellationToken);
            UpdateTimestamps();
            var result = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Saved {Count} changes to database, {EventCount} outbox events created", result, domainEvents.Count);
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

    /// <summary>
    /// Collects all domain events from tracked entities that implement IHasDomainEvents.
    /// </summary>
    private List<IDomainEvent> CollectDomainEvents()
    {
        var events = new List<IDomainEvent>();
        foreach (var entry in _context.ChangeTracker.Entries<IHasDomainEvents>())
        {
            events.AddRange(entry.Entity.DomainEvents);
            entry.Entity.ClearDomainEvents();
        }
        return events;
    }

    /// <summary>
    /// Creates OutboxEvent records from domain events for async dispatch.
    /// </summary>
    private async Task CreateOutboxEventsAsync(List<IDomainEvent> domainEvents, CancellationToken ct)
    {
        foreach (var de in domainEvents)
        {
            var outboxEvent = new OutboxEvent
            {
                TenantId = ExtractTenantId(de),
                EventType = de.GetType().Name,
                EventVersion = 1,
                AggregateType = de.GetType().Name.Replace("Event", ""),
                AggregateId = ExtractAggregateId(de),
                Payload = JsonSerializer.Serialize(de),
                Headers = JsonSerializer.Serialize(new { correlation_id = ExtractCorrelationId(de) }),
                Status = "pending",
                OccurredAt = de.OccurredAt,
                AvailableAt = DateTime.UtcNow
            };
            await _context.OutboxEvents.AddAsync(outboxEvent, ct);
        }
    }

    private static Guid? ExtractTenantId(IDomainEvent de)
    {
        try
        {
            var prop = de.GetType().GetProperty("TenantId");
            return prop?.GetValue(de) as Guid?;
        }
        catch { return null; }
    }

    private static string ExtractAggregateId(IDomainEvent de)
    {
        try
        {
            var prop = de.GetType().GetProperty("Id") ?? de.GetType().GetProperty("WalletId")
                ?? de.GetType().GetProperty("PaymentIntentId") ?? de.GetType().GetProperty("PaymentLinkId");
            return prop?.GetValue(de)?.ToString() ?? string.Empty;
        }
        catch { return string.Empty; }
    }

    private static string? ExtractCorrelationId(IDomainEvent de)
    {
        try
        {
            var prop = de.GetType().GetProperty("CorrelationId");
            return prop?.GetValue(de) as string;
        }
        catch { return null; }
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
