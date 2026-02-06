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
            .ThenInclude(s => s.SeatType)
            .Include(o => o.Session)
            .ThenInclude(s => s.Movie)
            .Include(o => o.Session)
            .ThenInclude(s => s.Hall)
            .Include(o => o.Payment)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByUserIdFilteredBySessionAsync(string userId, DateTime? from, DateTime? to,
        OrderStatus? status)
    {
        var query = context.Orders
            .Include(o => o.Tickets)
            .ThenInclude(t => t.SeatReservation)
            .ThenInclude(sr => sr.Seat)
            .ThenInclude(s => s.SeatType)
            .Include(o => o.Session)
            .ThenInclude(s => s.Movie)
            .Include(o => o.Session)
            .ThenInclude(s => s.Hall)
            .Include(o => o.Payment)
            .Where(o => o.UserId == userId)
            .AsQueryable();

        if (from.HasValue)
        {
            var fromDate = from.Value.Date;
            query = query.Where(o => o.Session.StartTime >= fromDate);
        }

        if (to.HasValue)
        {
            var toExclusive = to.Value.Date.AddDays(1);
            query = query.Where(o => o.Session.StartTime < toExclusive);
        }

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        return await query
            .OrderByDescending(o => o.Session.StartTime)
            .ToListAsync();
    }

    public async Task<int> CountByUserIdAsync(string userId)
    {
        return await context.Orders
            .Where(o => o.UserId == userId)
            .CountAsync();
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