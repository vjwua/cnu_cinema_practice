using Core.DTOs.Halls;
using Core.Entities;
using Core.Enums;
using Core.Mapping;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class HallRepository : IHallRepository
{
    private CinemaDbContext context;
    private DbSet<Hall> halls;

    public HallRepository(CinemaDbContext context)
    {
        this.context = context;
        this.halls = this.context.Halls;
    }

    public async Task<Hall?> GetByIdAsync(int id)
    {
        /*var searchResult = await halls
            .Where(h => h.Id == id)
            .ToListAsync();*/
        return halls.Where(h => h.Id == id).FirstOrDefault();
    }

    public async Task<Hall?> GetByIdWithLayoutAsync(int id)
    {
        return await GetByIdAsync(id);
    }

    public async Task<IEnumerable<Hall>> GetAllAsync()
    {
        var halls = await this.halls.ToListAsync();
        return halls.AsEnumerable();
        // вони вже всі є, то чи потрібно тут async functionality?
        return this.halls.AsEnumerable();
    }
    
    public async Task<Hall> CreateAsync(string name, SeatLayoutMap layout)
    {
        var addResult = await halls.AddAsync(new Hall(name, layout));
        var hall = addResult.Entity;
        GenerateSeatsForHallAsync(hall);
        return hall;
    }

    public async void GenerateSeatsForHallAsync(Hall hall)
    {
        SeatLayoutMap layout = SeatLayoutMap.FromByteArray(hall.SeatLayout);
        for (byte row = 0; row < 16; row++)
        {
            for (byte col = 0; col < 16; col++)
            {
                SeatType seat = layout.Get(row, col);
                if (seat != SeatType.Empty)
                {
                    context.Seats.AddAsync(new Seat()
                    {
                        RowNum = row,
                        SeatNum = col,
                        SeatType = seat,
                    });
                }
            }
        }
    }

    public async Task UpdateLayoutAsync(int hallId, Action<SeatLayoutMap> updateAction)
    {
        
    }

    public Task<bool> ExistsAsync(int id)
    {
        return halls.AnyAsync(h => h.Id == id);
    }
}