using Core.DTOs.Halls;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class HallRepository : IHallRepository
{
    private CinemaDbContext _context;
    private readonly DbSet<Hall> _halls;
    private readonly DbSet<Seat> _seats;
    private readonly DbSet<SeatType> _seatTypes;
    private readonly DbSet<SeatReservation> _seatReservations;
    private readonly DbSet<Session> _sessions;

    public HallRepository(CinemaDbContext context)
    {
        this._context = context;
        this._halls = this._context.Halls;
        this._seats = this._context.Seats;
        this._seatTypes = this._context.SeatTypes;
        this._seatReservations = this._context.SeatReservations;
        this._sessions = this._context.Sessions;
    }

    public async Task<Hall?> GetByIdAsync(int id)
    {
        var searchResult = await _halls
            .Where(h => h.Id == id)
            .ToListAsync();
        return searchResult.FirstOrDefault();
    }

    /*public async Task<Hall?> GetByIdWithLayoutAsync(int id)
    {
        return await GetByIdAsync(id);
    }*/

    public async Task<IEnumerable<Seat>> GetSeatsByHallIdAsync(int hallId)
    {
        var seatsByHall = await _seats
            .Where(s => s.HallId == hallId)
            .ToListAsync();
        return seatsByHall.AsEnumerable();
    }

    public async Task<IEnumerable<Hall>> GetAllAsync()
    {
        var halls = await this._halls.ToListAsync();
        return halls.AsEnumerable();
    }

    public async Task<Hall> CreateAsync(Hall hall)
    {
        var addResult = await _halls.AddAsync(hall);
        await _context.SaveChangesAsync();
        return addResult.Entity;
    }

    public async Task CreateSeatsAsync(int hallId, byte[,] layout)
    {
        byte rows = (byte) layout.GetLength(0);
        byte columns = (byte) layout.GetLength(1);
        Hall? hall = await GetByIdAsync(hallId);
        if (hall == null) return;
        hall.UpdateDimensions(rows, columns);
        List<SeatType> seatTList = await _seatTypes.ToListAsync();
        List<Seat> seatList = new List<Seat>();
        for (byte r = 0; r < rows; r++)
        {
            for (byte c = 0; c < columns; c++)
            {
                if (layout[r, c] != 0)
                {
                    SeatType? seatT = seatTList
                        .FirstOrDefault(st => st.Id == layout[r, c]);
                    if (seatT != null)
                    {
                        Seat newSeat = new Seat()
                        {
                            RowNum = r,
                            SeatNum = c,
                            Hall = hall,
                            HallId = hall.Id,
                            SeatTypeId = layout[r, c],
                            SeatType = seatT,
                        };
                        seatList.Add(newSeat);
                    }
                }
                
            }
        }
        await _seats.AddRangeAsync(seatList);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateSeatLayoutAsync(int hallId, byte[,] seatLayout)
    {
        await RemoveAllSeatsAsync(hallId);
        await CreateSeatsAsync(hallId, seatLayout);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateNameAsync(int hallId, string name)
    {
        Hall? hall = await GetByIdAsync(hallId);
        if (hall != null) hall.UpdateName(name);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateDimensionsAsync(int hallId, byte rows, byte cols)
    {
        Hall? hall = await GetByIdAsync(hallId);
        if (hall != null) hall.UpdateDimensions(rows, cols);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAllSeatsAsync(int hallId)
    {
        var seatsByHall = await _seats
            .Where(s => s.HallId == hallId)
            .ToListAsync();
        var seatReservations = await _seatReservations
            .Where(sr => seatsByHall.Contains(sr.Seat))
            .ToListAsync();
        _seatReservations.RemoveRange(seatReservations);
        
        var sessions = _sessions.Where(s => s.HallId == hallId).ToList();
        _sessions.RemoveRange(sessions);
        await _context.SaveChangesAsync();
        _seats.RemoveRange(seatsByHall);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetSeatCountAsync(int hallId)
    {
        int seatsByHall = await _seats
            .Where(s => s.HallId == hallId)
            .CountAsync();
        return seatsByHall;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        var result = await _halls.AnyAsync(h => h.Id == id);
        return result;
    }

    public async Task DeleteAsync(int id)
    {
        var hall = await GetByIdAsync(id);
        if (hall != null)
        {
            await RemoveAllSeatsAsync(id);
            _halls.Remove(hall);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}