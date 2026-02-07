using Core.Entities;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class TicketRepository(CinemaDbContext context) : ITicketRepository
{
    public Task<Ticket?> GetForScanAsync(int ticketId)
       => context.Tickets
            .Include(t => t.SeatReservation)
                .ThenInclude(sr => sr.Seat)
            .Include(t => t.Order)
                .ThenInclude(o => o.Session)
                    .ThenInclude(s => s.Movie)
            .Include(t => t.Order)
                .ThenInclude(o => o.Session)
                    .ThenInclude(s => s.Hall)
            .FirstOrDefaultAsync(t => t.Id == ticketId);  
    
    public Task SaveChangesAsync() => context.SaveChangesAsync();
}