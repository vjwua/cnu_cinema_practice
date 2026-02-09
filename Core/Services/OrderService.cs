using AutoMapper;
using Core.DTOs.Orders;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using System.Net.NetworkInformation;

namespace Core.Services;

public class OrderService(
    IOrderRepository orderRepository,
    ISeatReservationRepository reservationRepository,
    IMapper mapper) : IOrderService
{
    public async Task<OrderDTO> CreateOrderAsync(string userId, CreateOrderDTO dto)
    {
        var reservations = await reservationRepository.GetByIdsAsync(dto.SeatReservationIds);

        if (reservations.Count != dto.SeatReservationIds.Count)
            throw new KeyNotFoundException("One or more reservations not found.");

        if (reservations.Any(r => r.ReservedByUserId != userId))
            throw new UnauthorizedAccessException("You can only create orders for your own reservations.");

        if (reservations.Any(r => r.Status != ReservationStatus.Reserved ||
                                  (r.ExpiresAt.HasValue && r.ExpiresAt.Value < DateTime.UtcNow)))
        {
            throw new InvalidOperationException("Some reservations are expired or already sold.");
        }

        var sessionId = reservations.First().SessionId;
        if (reservations.Any(r => r.SessionId != sessionId))
            throw new InvalidOperationException("All tickets must be for the same session.");

        var order = new Order
        {
            UserId = userId,
            SessionId = sessionId,
            CreatedAt = DateTime.UtcNow,
            Status = OrderStatus.Pending
        };

        foreach (var reservation in reservations)
        {
            var ticket = new Ticket
            {
                SeatReservationId = reservation.Id,
                SeatReservation = reservation,
                Price = reservation.Price
            };
            order.Tickets.Add(ticket);
        }

        var createdOrder = await orderRepository.CreateAsync(order);

        return mapper.Map<OrderDTO>(createdOrder);
    }



    public async Task<OrderDTO> GetByIdAsync(int id)
    {
        var order = await orderRepository.GetByIdAsync(id);
        return mapper.Map<OrderDTO>(order);
    }

    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = await orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new KeyNotFoundException($"Order with ID {orderId} not found.");

        order.Status = status;
        await orderRepository.UpdateAsync(order);
    }

    public async Task<IEnumerable<OrderDTO>> GetUserOrdersAsync(string userId)
    {
        var orders = await orderRepository.GetByUserIdAsync(userId);
        return mapper.Map<IEnumerable<OrderDTO>>(orders);
    }

    public async Task<IEnumerable<OrderDTO>> GetUserOrdersFilteredBySessionAsync(string userId, DateTime? from, DateTime? to,
        OrderStatus? status)
    {
        var orders = await orderRepository.GetByUserIdFilteredBySessionAsync(userId, from, to, status);
        return mapper.Map<IEnumerable<OrderDTO>>(orders);
    }

    public async Task<int> CountUserOrdersAsync(string userId)
    {
        return await orderRepository.CountByUserIdAsync(userId);
    }

    public async Task CancelOrderAsync(int orderId)
    {
        var order = await orderRepository.GetByIdAsync(orderId);

        if (order == null)
            throw new KeyNotFoundException($"Order with ID {orderId} not found.");

        if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Expired)
            throw new InvalidOperationException($"Cannot cancel an order with status '{order.Status}'.");

        order.Status = OrderStatus.Cancelled;

        foreach (var ticket in order.Tickets)
        {
            if (ticket.SeatReservation != null)
            {
                ticket.SeatReservation.Status = ReservationStatus.Reserved;
                ticket.SeatReservation.ReservedByUserId = null;
                ticket.SeatReservation.ExpiresAt = null;
            }
        }

        await orderRepository.UpdateAsync(order);
    }
}