using Core.DTOs.Orders;
using Core.Enums;

namespace Core.Interfaces.Services;

public interface IOrderService
{
    Task<OrderDTO> CreateOrderAsync(string userId, CreateOrderDTO dto);
    Task<OrderDTO> GetByIdAsync(int id);
    Task UpdateOrderStatusAsync(int orderId, OrderStatus status);

    Task<IEnumerable<OrderDTO>> GetUserOrdersAsync(string userId);

    Task<IEnumerable<OrderDTO>> GetUserOrdersFilteredBySessionAsync(string userId, DateTime? from, DateTime? to,
        OrderStatus? status);

    Task<int> CountUserOrdersAsync(string userId);
    Task ExpireOrderAsync(int orderId);
    Task CancelOrderAsync(int orderId);
}