using Domain.Entities;

namespace Domain.Repositories;

public interface IPaymentLogRepository
{
    Task AddAsync(PaymentLog log);
    Task<List<PaymentLog>> GetByPaymentIdAsync(Guid paymentId);
}