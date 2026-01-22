using Core.DTOs.Halls;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class HallRepository : IHallRepository
{
    private CinemaDbContext _context;
    private readonly DbSet<Hall> _halls;
    private readonly DbSet<Seat> _seats;
    private readonly DbSet<SeatType> _seatTypes;

    public HallRepository(CinemaDbContext context)
    {
        this._context = context;
        this._halls = this._context.Halls;
        this._seats = this._context.Seats;
        this._seatTypes = this._context.SeatTypes;
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
        // вони вже всі є, то чи потрібно тут async functionality?
        return this._halls.AsEnumerable();
    }

    public async Task<Hall> CreateAsync(Hall hall)
    {
        var addResult = await _halls.AddAsync(hall);
        return addResult.Entity;
    }

    public async Task CreateSeatsAsync(int hallId, byte[,] layout)
    {
        byte rows = (byte) layout.GetLength(0);
        byte columns = (byte) layout.GetLength(1);
        Hall? hall = await GetByIdAsync(hallId);
        if (hall == null) return;
        hall.UpdateDimensions(rows, columns);
        for (byte r = 0; r < rows; r++)
        {
            for (byte c = 0; c < columns; c++)
            {
                if (layout[r, c] != 0)
                {
                    Seat newSeat = new Seat()
                    {
                        RowNum = r,
                        SeatNum = c,
                        Hall = hall,
                        HallId = hall.Id,
                        SeatTypeId = layout[r, c],
                        SeatType = _seatTypes
                            .Where(st => st.Id == layout[r, c])
                            .ToList()
                            .First(),
                    };
                    _seats.Add(newSeat);
                    hall.Seats.Add(newSeat);
                }
                
            }
        }
    }

    public async Task UpdateSeatLayoutAsync(int hallId, byte[,] seatLayout)
    {
        await RemoveAllSeatsAsync(hallId);
        await CreateSeatsAsync(hallId, seatLayout);
    }

    public async Task UpdateNameAsync(int hallId, string name)
    {
        Hall? hall = await GetByIdAsync(hallId);
        if (hall != null) hall.UpdateName(name);
    }

    public async Task RemoveAllSeatsAsync(int hallId)
    {
        var seatsByHall = await _seats
            .Where(s => s.HallId == hallId)
            .ToListAsync();
        foreach (var seat in seatsByHall)
        {
            _seats.Remove(seat);
        }

        Hall? hall = await GetByIdAsync(hallId);
        hall.Seats = new List<Seat>();
    }

    public async Task<int> GetSeatCountAsync(int hallId)
    {
        var seatsByHall = await GetSeatsByHallIdAsync(hallId);
        return seatsByHall.Count();
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
        }
    }
}