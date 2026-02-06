using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Parbad.Storage.Abstractions;
using ParbadPaymentEntity = Parbad.Storage.Abstractions.Models.Payment;
using ParbadTransactionEntity = Parbad.Storage.Abstractions.Models.Transaction;

namespace Infrastructure.Data;

public class PaymentStorageRepository : IStorage
{
    public IQueryable<ParbadPaymentEntity> Payments { get; }
    public IQueryable<ParbadTransactionEntity> Transactions { get; }

    private readonly PaymentDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PaymentStorageRepository(PaymentDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;

        Payments = _context.ParbadPayments;
        Transactions = _context.ParbadTransactions;
    }

    public async Task CreatePaymentAsync(ParbadPaymentEntity payment, CancellationToken cancellationToken = default)
    {
        await _context.ParbadPayments.AddAsync(payment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdatePaymentAsync(ParbadPaymentEntity payment, CancellationToken cancellationToken = default)
    {
        var record = await _context.ParbadPayments.SingleOrDefaultAsync(model => model.Id == payment.Id, cancellationToken);
        if (record == null) throw new Exception("Payment not found");

        record.Token = payment.Token;
        record.TrackingNumber = payment.TrackingNumber;
        record.TransactionCode = payment.TransactionCode;

        _context.ParbadPayments.Update(record);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePaymentAsync(ParbadPaymentEntity payment, CancellationToken cancellationToken = default)
    {
        var record = await _context.ParbadPayments.SingleOrDefaultAsync(model => model.Id == payment.Id, cancellationToken);
        if (record == null) throw new Exception("Payment not found");

        _context.ParbadPayments.Remove(record);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateTransactionAsync(ParbadTransactionEntity transaction, CancellationToken cancellationToken = default)
    {
        await _context.ParbadTransactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateTransactionAsync(ParbadTransactionEntity transaction, CancellationToken cancellationToken = default)
    {
        var record = await _context.ParbadTransactions.SingleOrDefaultAsync(model => model.Id == transaction.Id, cancellationToken);
        if (record == null) throw new Exception("Transaction not found");

        record.IsSucceed = transaction.IsSucceed;
        _context.ParbadTransactions.Update(record);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTransactionAsync(ParbadTransactionEntity transaction, CancellationToken cancellationToken = default)
    {
        var record = await _context.ParbadTransactions.SingleOrDefaultAsync(model => model.Id == transaction.Id, cancellationToken);
        if (record == null) throw new Exception("Transaction not found");

        _context.ParbadTransactions.Remove(record);
        await _context.SaveChangesAsync(cancellationToken);
    }
}