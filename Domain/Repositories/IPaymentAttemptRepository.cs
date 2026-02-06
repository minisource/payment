using Domain.Entities;

namespace Domain.Repositories;

public interface IPaymentAttemptRepository
{
    Task AddAsync(PaymentAttempt attempt);
    Task<List<PaymentAttempt>> GetByPaymentIdAsync(Guid paymentId);
}