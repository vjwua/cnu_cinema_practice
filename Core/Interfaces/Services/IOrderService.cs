using Core.DTOs.Orders;

namespace Core.Interfaces.Services;

public interface IOrderService
{
    Task<OrderDTO> CreateOrderAsync(string userId, CreateOrderDTO dto);
    Task<OrderDTO> GetOrderByIdAsync(int id);
    Task<IEnumerable<OrderDTO>> GetUserOrdersAsync(string userId);
}