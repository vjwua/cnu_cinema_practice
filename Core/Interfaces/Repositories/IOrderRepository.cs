using Core.Entities;
using Core.Enums;

namespace Core.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<Order?> GetByIdWithDetailsAsync(int id); // з Tickets, Session, Payment
    Task<IEnumerable<Order>> GetByUserIdAsync(string userId);
    Task<Order> CreateAsync(Order order);
    Task UpdateStatusAsync(int orderId, OrderStatus status);
    Task<IEnumerable<Order>> GetPendingOrdersAsync();
}