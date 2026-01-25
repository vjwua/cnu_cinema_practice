using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SeatReservationRepository(CinemaDbContext context) : ISeatReservationRepository
{
    public async Task<List<SeatReservation>> GetByIdsAsync(List<int> ids)
    {
        return await context.SeatReservations
            .Include(sr => sr.Seat)
            .Where(sr => ids.Contains(sr.Id))
            .ToListAsync();
    }

    public async Task MarkAsSoldAsync(List<int> ids)
    {
        await context.SeatReservations
            .Where(sr => ids.Contains(sr.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, ReservationStatus.Sold));
    }
}