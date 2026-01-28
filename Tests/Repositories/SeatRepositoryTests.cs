using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Core.Entities;
using Core.Enums;
using FluentAssertions;

namespace Tests.Repositories;

public class SeatRepositoryTests
{
    private CinemaDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CinemaDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CinemaDbContext(options);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSeat_WhenExists()
    {
        var dbName = Guid.NewGuid().ToString();
        int seatId;
        
        await using (var context = GetDbContext(dbName))
        {
            var hall = new Hall("Hall 1", 5, 5);
            context.Halls.Add(hall);
            await context.SaveChangesAsync();

            var seat = new Seat { HallId = hall.Id, RowNum = 1, SeatNum = 1 };
            context.Seats.Add(seat);
            await context.SaveChangesAsync();
            seatId = seat.Id;
        }

        await using (var context = GetDbContext(dbName))
        {
            var hallRepo = new HallRepository(context); 
            var seatRepo = new SeatRepository(context, hallRepo);

            var result = await seatRepo.GetByIdAsync(seatId);

            result.Should().NotBeNull();
            result.SeatNum.Should().Be(1);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenSeatDoesNotExist()
    {
        var dbName = Guid.NewGuid().ToString();

        await using (var context = GetDbContext(dbName))
        {
            var hallRepo = new HallRepository(context);
            var seatRepo = new SeatRepository(context, hallRepo);

            var result = await seatRepo.GetByIdAsync(999);

            result.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetBySessionIdAsync_ShouldReturnAllSeatsInHall()
    {
        var dbName = Guid.NewGuid().ToString();
        int sessionId;

        await using (var context = GetDbContext(dbName))
        {
            var hall = new Hall("Hall 1", 1, 2);
            context.Halls.Add(hall);
            await context.SaveChangesAsync();

            var seat1 = new Seat { HallId = hall.Id, RowNum = 1, SeatNum = 1 };
            var seat2 = new Seat { HallId = hall.Id, RowNum = 1, SeatNum = 2 };
            context.Seats.AddRange(seat1, seat2);

            var session = new Session { HallId = hall.Id, MovieId = 1 };
            context.Sessions.Add(session);
            await context.SaveChangesAsync();
            sessionId = session.Id;
        }

        await using (var context = GetDbContext(dbName))
        {
            var hallRepo = new HallRepository(context);
            var seatRepo = new SeatRepository(context, hallRepo);

            var result = await seatRepo.GetBySessionIdAsync(sessionId);

            result.Should().HaveCount(2);
        }
    }

    [Fact]
    public async Task GetAvailableSeatsAsync_ShouldReturnOnlyUnreservedSeats()
    {
        var dbName = Guid.NewGuid().ToString();
        int sessionId;
        int freeSeatId;

        await using (var context = GetDbContext(dbName))
        {
            var hall = new Hall("Hall 1", 5, 5);
            context.Halls.Add(hall);
            await context.SaveChangesAsync();

            var seat1 = new Seat { HallId = hall.Id, RowNum = 1, SeatNum = 1 };
            var seat2 = new Seat { HallId = hall.Id, RowNum = 1, SeatNum = 2 };
            context.Seats.AddRange(seat1, seat2);

            var session = new Session { HallId = hall.Id, MovieId = 1 };
            context.Sessions.Add(session);
            await context.SaveChangesAsync();
            
            sessionId = session.Id;
            freeSeatId = seat2.Id;

            context.SeatReservations.Add(new SeatReservation 
            { 
                SessionId = sessionId, 
                SeatId = seat1.Id, 
                Status = ReservationStatus.Reserved 
            });
            await context.SaveChangesAsync();
        }

        await using (var context = GetDbContext(dbName))
        {
            var hallRepo = new HallRepository(context);
            var seatRepo = new SeatRepository(context, hallRepo);

            var result = (await seatRepo.GetAvailableSeatsAsync(sessionId)).ToList();

            result.Should().ContainSingle();
            result.First().Id.Should().Be(freeSeatId);
        }
    }

    [Fact]
    public async Task IsSeatAvailableAsync_ShouldReturnTrue_WhenSeatIsFree()
    {
        var dbName = Guid.NewGuid().ToString();
        int sessionId;
        int seatId;

        await using (var context = GetDbContext(dbName))
        {
            var hall = new Hall("Small Hall", 1, 1);
            context.Halls.Add(hall);
            await context.SaveChangesAsync();

            var seat = new Seat { HallId = hall.Id, RowNum = 1, SeatNum = 1 };
            context.Seats.Add(seat);

            var session = new Session { HallId = hall.Id, MovieId = 1 };
            context.Sessions.Add(session);
            
            await context.SaveChangesAsync();
            seatId = seat.Id;
            sessionId = session.Id;
        }

        await using (var context = GetDbContext(dbName))
        {
            var hallRepo = new HallRepository(context);
            var seatRepo = new SeatRepository(context, hallRepo);

            var result = await seatRepo.IsSeatAvailableAsync(seatId, sessionId);

            result.Should().BeTrue();
        }
    }

    [Fact]
    public async Task IsSeatAvailableAsync_ShouldReturnFalse_WhenSeatIsReserved()
    {
        var dbName = Guid.NewGuid().ToString();
        int sessionId;
        int seatId;

        await using (var context = GetDbContext(dbName))
        {
            var hall = new Hall("Small Hall", 1, 1);
            context.Halls.Add(hall);
            await context.SaveChangesAsync();

            var seat = new Seat { HallId = hall.Id, RowNum = 1, SeatNum = 1 };
            context.Seats.Add(seat);

            var session = new Session { HallId = hall.Id, MovieId = 1 };
            context.Sessions.Add(session);
            await context.SaveChangesAsync();
            
            seatId = seat.Id;
            sessionId = session.Id;

            context.SeatReservations.Add(new SeatReservation 
            { 
                SessionId = sessionId, 
                SeatId = seatId, 
                Status = ReservationStatus.Reserved 
            });
            await context.SaveChangesAsync();
        }

        await using (var context = GetDbContext(dbName))
        {
            var hallRepo = new HallRepository(context);
            var seatRepo = new SeatRepository(context, hallRepo);

            var result = await seatRepo.IsSeatAvailableAsync(seatId, sessionId);

            result.Should().BeFalse();
        }
    }

    [Fact]
    public async Task ReserveSeatAsync_ShouldReturnTrue_WhenSeatIsAvailable()
    {
        var dbName = Guid.NewGuid().ToString();
        int sessionId;
        int seatId;
        int hallId; 

        await using (var context = GetDbContext(dbName))
        {
            var hall = new Hall("Hall 1", 5, 5); 
            context.Halls.Add(hall);
            await context.SaveChangesAsync(); 
            hallId = hall.Id;

            var seat = new Seat { HallId = hallId, RowNum = 1, SeatNum = 1 };
            context.Seats.Add(seat);

            var session = new Session { HallId = hallId, MovieId = 1 };
            context.Sessions.Add(session);

            await context.SaveChangesAsync(); 
            seatId = seat.Id;
            sessionId = session.Id;
        }

        await using (var context = GetDbContext(dbName))
        {
            var hallRepo = new HallRepository(context);
            var seatRepo = new SeatRepository(context, hallRepo);

            bool result = await seatRepo.ReserveSeatAsync(seatId, sessionId);
            await context.SaveChangesAsync();

            result.Should().BeTrue(); 
            
            var reservation = await context.SeatReservations.FirstOrDefaultAsync();
            reservation.Should().NotBeNull();
            reservation.SeatId.Should().Be(seatId);
        }
    }

    [Fact]
    public async Task ReserveSeatAsync_ShouldReturnFalse_WhenSeatAlreadyReserved()
    {
        var dbName = Guid.NewGuid().ToString();
        int sessionId;
        int seatId;

        await using (var context = GetDbContext(dbName))
        {
            var hall = new Hall("Hall 1", 5, 5);
            context.Halls.Add(hall);
            await context.SaveChangesAsync();

            var seat = new Seat { HallId = hall.Id, RowNum = 1, SeatNum = 1 };
            context.Seats.Add(seat);

            var session = new Session { HallId = hall.Id, MovieId = 1 };
            context.Sessions.Add(session);
            
            await context.SaveChangesAsync();
            seatId = seat.Id;
            sessionId = session.Id;

            context.SeatReservations.Add(new SeatReservation 
            { 
                SessionId = sessionId, 
                SeatId = seatId, 
                Status = ReservationStatus.Reserved 
            });
            await context.SaveChangesAsync();
        }

        await using (var context = GetDbContext(dbName))
        {
            var hallRepo = new HallRepository(context);
            var seatRepo = new SeatRepository(context, hallRepo);

            bool result = await seatRepo.ReserveSeatAsync(seatId, sessionId);

            result.Should().BeFalse();
        }
    }
}