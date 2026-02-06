using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Infrastructure.Data;
using Infrastructure.Identity;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace cnu_cinema_practice.Controllers;

public class ReservationCleanup : IHostedService
{
    private Timer? _timer;

    private async void Cleanup(object? state)
    {
        try
        {
            var contextOptions = new DbContextOptionsBuilder<CinemaDbContext>()
                .UseSqlServer("Server=172.18.0.2,1433;Database=CinemaDb;User Id=sa;Password=Password123!;TrustServerCertificate=True;")
                .Options;
            var context = new CinemaDbContext(contextOptions);
            await CleanupExpiredAsync(context);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(Cleanup, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }
    
    private async Task<IEnumerable<SeatReservation>> GetExpiredAsync(CinemaDbContext context)
    {
        var reservations = await context.SeatReservations.Where(sr =>
            sr.Status == ReservationStatus.Reserved && sr.ExpiresAt < DateTime.Now).ToListAsync();
        return reservations.AsEnumerable();
    }

    private async Task CleanupExpiredAsync(CinemaDbContext context)
    {
        var toCleanup = await GetExpiredAsync(context);
        context.SeatReservations.RemoveRange(toCleanup);
        await context.SaveChangesAsync();
    }
}