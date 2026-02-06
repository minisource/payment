using Domain.Entities;
using Domain.Repositories;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using PaymentEntity = Domain.Entities.Payment;

namespace Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Payment aggregate.
/// </summary>
public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;
    private static long _trackingNumberSequence = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    private static readonly object _trackingLock = new();

    public PaymentRepository(PaymentDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<PaymentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Attempts)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PaymentEntity?> GetByIdWithLogsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Attempts)
            .Include(p => p.Logs)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PaymentEntity?> GetByTrackingNumberAsync(long trackingNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Attempts)
            .Include(p => p.Logs)
            .FirstOrDefaultAsync(p => p.TrackingNumber == trackingNumber, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PaymentEntity?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task AddAsync(PaymentEntity payment, CancellationToken cancellationToken = default)
    {
        await _context.Payments.AddAsync(payment, cancellationToken);
    }

    /// <inheritdoc/>
    public Task UpdateAsync(PaymentEntity payment, CancellationToken cancellationToken = default)
    {
        _context.Payments.Update(payment);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task UpdateTrackingNumberAsync(Guid paymentId, long trackingNumber, CancellationToken cancellationToken = default)
    {
        await _context.Payments
            .Where(p => p.Id == paymentId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.TrackingNumber, trackingNumber), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<PaymentEntity>> GetPaymentsAsync(
        string? userId,
        string? status,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        PaymentStatus? parsedStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<PaymentStatus>(status, true, out var s))
        {
            parsedStatus = s;
        }

        var query = BuildFilterQuery(parsedStatus, userId, from, to);

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalCountAsync(
        string? userId,
        string? status,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        PaymentStatus? parsedStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<PaymentStatus>(status, true, out var s))
        {
            parsedStatus = s;
        }

        var query = BuildFilterQuery(parsedStatus, userId, from, to);
        return await query.CountAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<PaymentEntity>> GetByUserIdAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<long> GenerateTrackingNumberAsync(CancellationToken cancellationToken = default)
    {
        // Thread-safe tracking number generation
        lock (_trackingLock)
        {
            _trackingNumberSequence++;
            return Task.FromResult(_trackingNumberSequence);
        }
    }

    private IQueryable<PaymentEntity> BuildFilterQuery(
        PaymentStatus? status,
        string? userId,
        DateTime? from,
        DateTime? to)
    {
        var query = _context.Payments.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(p => p.UserId == userId);
        }

        if (from.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= to.Value);
        }

        return query;
    }
}