using Core.Entities;
using Core.Enums;

namespace Core.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetByUserIdAsync(string userId);
    Task<Order> CreateAsync(Order order);
    Task UpdateAsync(Order order);
    Task<IEnumerable<Order>> GetExpiredPendingOrdersAsync(DateTime cutoffTime);
    Task<IEnumerable<Order>> GetOrdersBySessionIdAsync(int sessionId);
}