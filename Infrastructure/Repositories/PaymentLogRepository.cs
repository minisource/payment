using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PaymentLogRepository : IPaymentLogRepository
{
    private readonly PaymentDbContext _context;

    public PaymentLogRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PaymentLog log)
    {
        await _context.PaymentLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<PaymentLog>> GetByPaymentIdAsync(Guid paymentId)
    {
        return await _context.PaymentLogs
            .Where(l => l.PaymentId == paymentId)
            .OrderBy(l => l.Timestamp)
            .ToListAsync();
    }
}