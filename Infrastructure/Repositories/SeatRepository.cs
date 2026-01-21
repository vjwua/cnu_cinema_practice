using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Interfaces;

public class SeatRepository : ISeatRepository
{
    private CinemaDbContext context;
    private DbSet<Seat> seats;

    public SeatRepository(CinemaDbContext con)
    {
        this.context = con;
        this.seats = this.context.Seats;
    }
    
    public async Task<Seat?> GetByIdAsync(int id)
    {
        var searchResult = await seats
            .Where(s => s.Id == id)
            .ToListAsync();
        if (searchResult.Count > 0) return searchResult.First();
        return null;
    }
    
    public async Task<IEnumerable<Seat>> GetBySessionIdAsync(int sessionId)
    {
        var seatsBySession = await seats
            .Where(s => sessionId == sessionId)
            .ToListAsync();
        return seatsBySession.AsEnumerable();
    }
    
    public async Task<IEnumerable<Seat>> GetAvailableSeatsAsync(int sessionId)
    {
        var seatsBySession = await seats
            .Where(s => s.SessionId == sessionId && s.IsAvailable)
            .ToListAsync();
        return seatsBySession.AsEnumerable();
    } // перегляд доступних місць
    
    public async Task<bool> ReserveSeatAsync(int seatId)
    {
        var seat = await GetByIdAsync(seatId);
        if (seat == null || seat.IsAvailable == false) return false;

        seat.IsAvailable = false;
        return true;
    } // для покупки
    
    public async Task<bool> IsSeatAvailableAsync(int seatId)
    {
        var result = await seats
            .Where(s => s.Id == seatId)
            .ToListAsync();
        return result.First().IsAvailable;
    }
}