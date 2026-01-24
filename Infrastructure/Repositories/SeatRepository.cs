using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Interfaces;

public class SeatRepository : ISeatRepository
{
    private CinemaDbContext _context;
    private readonly HallRepository _hallRepo;
    private readonly DbSet<Seat> _seats;
    private readonly DbSet<Session> _sessions;
    private readonly DbSet<SeatReservation> _seatReservations;

    public SeatRepository(CinemaDbContext con, HallRepository hRepo)
    {
        this._context = con;
        this._seats = this._context.Seats;
        this._sessions = this._context.Sessions;
        this._hallRepo = hRepo;
        this._seatReservations = this._context.SeatReservations;
    }
    
    public async Task<Seat?> GetByIdAsync(int id)
    {
        var searchResult = await _seats
            .Where(s => s.Id == id)
            .ToListAsync();
        return searchResult.FirstOrDefault();
    }
    
    public async Task<IEnumerable<Seat>> GetBySessionIdAsync(int sessionId)
    {
        var hallBySession = await _sessions
            .Where(s => s.Id == sessionId)
            .Select(s => s.HallId)
            .ToListAsync();
        var hallId = hallBySession.FirstOrDefault();
        var seatsByHall = await _hallRepo.GetSeatsByHallIdAsync(hallId);
        return seatsByHall;
    }
    
    public async Task<IEnumerable<Seat>> GetAvailableSeatsAsync(int sessionId)
    {
        IEnumerable<Seat> seatEnumerable = await GetBySessionIdAsync(sessionId);
        List<Seat> seatsBySession = seatEnumerable.ToList();
        List<int> allSeatIds = seatsBySession.Select(s => s.Id).ToList();
        List<int> reservedSeatIds = await _seatReservations
            .Where(sr => sr.SessionId == sessionId)
            .Select(sr => sr.SeatId)
            .ToListAsync();
        List<int> availiableSeatIds = allSeatIds.Where(si => reservedSeatIds.Contains(si) == false).ToList();
        List<Seat> availiableSeats = seatsBySession
            .Where(s => availiableSeatIds.Contains(s.Id))
            .ToList();
        return availiableSeats.AsEnumerable();
    } // перегляд доступних місць
    
    public async Task<bool> ReserveSeatAsync(int seatId, int sessionId)
    {
        var isAvailiable = await IsSeatAvailableAsync(seatId, sessionId);
        if (isAvailiable == false) return false;
        
        _seatReservations.Add(new SeatReservation()
        {
            SessionId = sessionId,
            SeatId = seatId,
            Status = ReservationStatus.Reserved,
        });
        await _context.SaveChangesAsync();
        return true;
    } // для покупки
    
    public async Task<bool> IsSeatAvailableAsync(int seatId, int sessionId)
    {
        var availableForSession = await GetAvailableSeatsAsync(sessionId);
        var result = availableForSession.Contains(await GetByIdAsync(seatId));
        return result;
    }
}