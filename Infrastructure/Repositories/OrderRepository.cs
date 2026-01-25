using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OrderRepository(CinemaDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(int id)
    {
        return await context.Orders
            .Include(o => o.Tickets)
                .ThenInclude(t => t.SeatReservation)
                    .ThenInclude(sr => sr.Seat)
            .Include(o => o.Session)
                .ThenInclude(s => s.Movie)
            .Include(o => o.Session)
                .ThenInclude(s => s.Hall)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(string userId)
    {
        return await context.Orders
            .Include(o => o.Tickets)
                .ThenInclude(t => t.SeatReservation)
                    .ThenInclude(sr => sr.Seat)
            .Include(o => o.Session)
                .ThenInclude(s => s.Movie)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order> CreateAsync(Order order)
    {
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();
        return order;
    }

    public async Task UpdateAsync(Order order)
    {
        context.Orders.Update(order);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Order>> GetExpiredPendingOrdersAsync(DateTime cutoffTime)
    {
        return await context.Orders
            .Include(o => o.Tickets)
            .Where(o => o.Status == OrderStatus.Pending && o.CreatedAt < cutoffTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersBySessionIdAsync(int sessionId)
    {
        return await context.Orders
            .Include(o => o.Tickets)
            .Where(o => o.SessionId == sessionId)
            .ToListAsync();
    }
}