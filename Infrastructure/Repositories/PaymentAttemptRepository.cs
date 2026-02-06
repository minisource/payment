using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PaymentAttemptRepository : IPaymentAttemptRepository
{
    private readonly PaymentDbContext _context;

    public PaymentAttemptRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PaymentAttempt attempt)
    {
        await _context.PaymentAttempts.AddAsync(attempt);
        await _context.SaveChangesAsync();
    }

    public async Task<List<PaymentAttempt>> GetByPaymentIdAsync(Guid paymentId)
    {
        return await _context.PaymentAttempts
            .Where(a => a.PaymentId == paymentId)
            .OrderBy(a => a.AttemptNumber)
            .ToListAsync();
    }
}