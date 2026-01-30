using Core.Entities;

namespace Core.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByOrderIdAsync(int orderId);
    Task<Payment> CreateAsync(Payment payment);
}