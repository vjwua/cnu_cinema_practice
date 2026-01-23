using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Core.Entities;
using FluentAssertions;

namespace Tests.Repositories;

public class SessionRepositoryTests
{
    private CinemaDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CinemaDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CinemaDbContext(options);
    }

    private async Task<(int MovieId, int HallId)> CreateDependenciesAsync(CinemaDbContext context)
    {
        var movie = new Movie 
        { 
            Name = "Test Movie", 
            ReleaseDate = DateOnly.FromDateTime(DateTime.Now) 
        };
        
        var hall = new Hall("Test Hall", 10, 10);
        
        if (!await context.SeatTypes.AnyAsync())
        {
            context.SeatTypes.Add(new SeatType { Id = 1, Name = "Standard" });
        }

        context.Movies.Add(movie);
        context.Halls.Add(hall);
        
        await context.SaveChangesAsync();
        
        return (movie.Id, hall.Id);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddSession()
    {
        var dbName = Guid.NewGuid().ToString();

        int movieId, hallId;
        await using (var context = GetDbContext(dbName))
        {
            (movieId, hallId) = await CreateDependenciesAsync(context);
            var repo = new SessionRepository(context);
            
            var session = new Session 
            { 
                StartTime = DateTime.UtcNow.AddHours(5), 
                MovieId = movieId, 
                HallId = hallId 
            };

            await repo.CreateAsync(session);
        }

        await using (var context = GetDbContext(dbName))
        {
            var savedSession = await context.Sessions.FirstOrDefaultAsync();
            savedSession.Should().NotBeNull();
            savedSession.HallId.Should().Be(hallId);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSession_WhenExists()
    {
        var dbName = Guid.NewGuid().ToString();
        int sessionId;

        await using (var context = GetDbContext(dbName))
        {
            var (movieId, hallId) = await CreateDependenciesAsync(context);
            var session = new Session { StartTime = DateTime.UtcNow, MovieId = movieId, HallId = hallId };
            context.Sessions.Add(session);
            await context.SaveChangesAsync();
            sessionId = session.Id;
        }

        await using (var context = GetDbContext(dbName))
        {
            var repo = new SessionRepository(context);
            var result = await repo.GetByIdAsync(sessionId);

            result.Should().NotBeNull();
            result.Id.Should().Be(sessionId);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenSessionDoesNotExist()
    {
        var dbName = Guid.NewGuid().ToString();

        await using (var context = GetDbContext(dbName))
        {
            var repo = new SessionRepository(context);
            var result = await repo.GetByIdAsync(999);

            result.Should().BeNull();
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifySession()
    {
        var dbName = Guid.NewGuid().ToString();
        int sessionId;
        var newTime = DateTime.UtcNow.AddDays(5);

        await using (var context = GetDbContext(dbName))
        {
            var (movieId, hallId) = await CreateDependenciesAsync(context);
            var session = new Session { StartTime = DateTime.UtcNow, MovieId = movieId, HallId = hallId };
            context.Sessions.Add(session);
            await context.SaveChangesAsync();
            sessionId = session.Id;
        }

        await using (var context = GetDbContext(dbName))
        {
            var repo = new SessionRepository(context);
            var sessionToUpdate = await repo.GetByIdAsync(sessionId);
            
            if (sessionToUpdate != null)
            {
                sessionToUpdate.StartTime = newTime;
                await repo.UpdateAsync(sessionToUpdate);
            }
        }

        await using (var context = GetDbContext(dbName))
        {
            var updatedSession = await context.Sessions.FindAsync(sessionId);
            updatedSession.Should().NotBeNull();
            updatedSession.StartTime.Should().BeCloseTo(newTime, TimeSpan.FromSeconds(1));
        }
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveSession_WhenExists()
    {
        var dbName = Guid.NewGuid().ToString();
        int sessionId;

        await using (var context = GetDbContext(dbName))
        {
            var (movieId, hallId) = await CreateDependenciesAsync(context);
            var session = new Session { StartTime = DateTime.UtcNow, MovieId = movieId, HallId = hallId };
            context.Sessions.Add(session);
            await context.SaveChangesAsync();
            sessionId = session.Id;
        }

        await using (var context = GetDbContext(dbName))
        {
            var repo = new SessionRepository(context);
            await repo.DeleteAsync(sessionId);
        }

        await using (var context = GetDbContext(dbName))
        {
            var deletedSession = await context.Sessions.FindAsync(sessionId);
            deletedSession.Should().BeNull();
        }
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotThrow_WhenSessionDoesNotExist()
    {
        var dbName = Guid.NewGuid().ToString();

        await using (var context = GetDbContext(dbName))
        {
            var repo = new SessionRepository(context);
            var action = async () => await repo.DeleteAsync(999);
            await action.Should().NotThrowAsync();
        }
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllSessions()
    {
        var dbName = Guid.NewGuid().ToString();

        await using (var context = GetDbContext(dbName))
        {
            var (movieId, hallId) = await CreateDependenciesAsync(context);
            context.Sessions.Add(new Session { StartTime = DateTime.UtcNow, MovieId = movieId, HallId = hallId });
            context.Sessions.Add(new Session { StartTime = DateTime.UtcNow, MovieId = movieId, HallId = hallId });
            await context.SaveChangesAsync();
        }

        await using (var context = GetDbContext(dbName))
        {
            var repo = new SessionRepository(context);
            var result = (await repo.GetAllAsync()).ToList();

            result.Should().HaveCount(2);
        }
    }

    [Fact]
    public async Task GetByMovieIdAsync_ShouldReturnCorrectSessions()
    {
        var dbName = Guid.NewGuid().ToString();
        int targetMovieId;

        await using (var context = GetDbContext(dbName))
        {
            var movie1 = new Movie { Name = "M1", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
            var movie2 = new Movie { Name = "M2", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
            var hall = new Hall("H1", 5, 5);
            
            context.Movies.AddRange(movie1, movie2);
            context.Halls.Add(hall);
            await context.SaveChangesAsync();
            
            targetMovieId = movie1.Id;

            context.Sessions.AddRange(
                new Session { MovieId = targetMovieId, HallId = hall.Id }, 
                new Session { MovieId = targetMovieId, HallId = hall.Id }, 
                new Session { MovieId = movie2.Id, HallId = hall.Id }
            );
            await context.SaveChangesAsync();
        }

        await using (var context = GetDbContext(dbName))
        {
            var repo = new SessionRepository(context);
            var result = (await repo.GetByMovieIdAsync(targetMovieId)).ToList();

            result.Should().HaveCount(2);
            result.All(s => s.MovieId == targetMovieId).Should().BeTrue();
        }
    }

    [Fact]
    public async Task GetByHallIdAsync_ShouldReturnCorrectSessions()
    {
        var dbName = Guid.NewGuid().ToString();
        int targetHallId;

        await using (var context = GetDbContext(dbName))
        {
            var movie = new Movie { Name = "M1", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
            var hall1 = new Hall("Target Hall", 5, 5);
            var hall2 = new Hall("Other Hall", 5, 5);
            
            context.Movies.Add(movie);
            context.Halls.AddRange(hall1, hall2);
            await context.SaveChangesAsync();

            targetHallId = hall1.Id;

            context.Sessions.AddRange(
                new Session { MovieId = movie.Id, HallId = targetHallId },
                new Session { MovieId = movie.Id, HallId = hall2.Id }
            );
            await context.SaveChangesAsync();
        }

        await using (var context = GetDbContext(dbName))
        {
            var repo = new SessionRepository(context);
            var result = (await repo.GetByHallIdAsync(targetHallId)).ToList();

            result.Should().HaveCount(1);
            result.First().HallId.Should().Be(targetHallId);
        }
    }

    [Fact]
    public async Task GetUpcomingSessionsAsync_ShouldReturnOnlyFutureSessions()
    {
        var dbName = Guid.NewGuid().ToString();
        var futureDate = DateTime.UtcNow.AddDays(1); 
        var pastDate = DateTime.UtcNow.AddDays(-1);

        await using (var context = GetDbContext(dbName))
        {
            var (movieId, hallId) = await CreateDependenciesAsync(context);
            context.Sessions.AddRange(
                new Session { StartTime = pastDate, MovieId = movieId, HallId = hallId },
                new Session { StartTime = futureDate, MovieId = movieId, HallId = hallId }
            );
            await context.SaveChangesAsync();
        }

        await using (var context = GetDbContext(dbName))
        {
            var repo = new SessionRepository(context);
            var result = (await repo.GetUpcomingSessionsAsync()).ToList();

            result.Should().HaveCount(1);
            result.First().StartTime.Should().BeCloseTo(futureDate, TimeSpan.FromSeconds(1));
        }
    }

    [Fact]
    public async Task GetByDateRangeAsync_ShouldReturnSessionsInsideRange()
    {
        var dbName = Guid.NewGuid().ToString();
        var today = DateTime.UtcNow;

        await using (var context = GetDbContext(dbName))
        {
            var (movieId, hallId) = await CreateDependenciesAsync(context);
            context.Sessions.AddRange(
                new Session { StartTime = today.AddDays(-10), MovieId = movieId, HallId = hallId }, 
                new Session { StartTime = today, MovieId = movieId, HallId = hallId },              
                new Session { StartTime = today.AddDays(10), MovieId = movieId, HallId = hallId }   
            );
            await context.SaveChangesAsync();
        }

        await using (var context = GetDbContext(dbName))
        {
            var repo = new SessionRepository(context);
            var result = (await repo.GetByDateRangeAsync(today.AddDays(-1), today.AddDays(1))).ToList();

            result.Should().HaveCount(1);
        }
    }

    [Fact]
    public async Task GetByIdWithSeatsAsync_ShouldIncludeSeatsAndTypes()
    {
        var dbName = Guid.NewGuid().ToString();
        int sessionId;

        await using (var context = GetDbContext(dbName))
        {
            var seatType = new SeatType { Id = 1, Name = "VIP" };
            var hall = new Hall("Big Hall", 1, 1);
            var movie = new Movie { Name = "Test Movie", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
            
            if (!await context.SeatTypes.AnyAsync(st => st.Id == 1))
            {
                context.SeatTypes.Add(seatType);
            }
            
            context.Halls.Add(hall);
            context.Movies.Add(movie);
            await context.SaveChangesAsync();

            var seat = new Seat { HallId = hall.Id, RowNum = 1, SeatNum = 1, SeatTypeId = 1 };
            context.Seats.Add(seat);
            
            var session = new Session { HallId = hall.Id, MovieId = movie.Id, StartTime = DateTime.UtcNow };
            context.Sessions.Add(session);
            
            await context.SaveChangesAsync();
            sessionId = session.Id;
        }

        await using (var context = GetDbContext(dbName))
        {
            var repo = new SessionRepository(context);
            var result = await repo.GetByIdWithSeatsAsync(sessionId);

            result.Should().NotBeNull();
            result.Hall.Should().NotBeNull();
            result.Hall.Seats.Should().NotBeNullOrEmpty();
            
            var loadedSeat = result.Hall.Seats.First();
            loadedSeat.SeatNum.Should().Be(1);
            loadedSeat.SeatType.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task HasAnyOrdersAsync_ShouldReturnTrue_WhenOrderExists()
    {
        var dbName = Guid.NewGuid().ToString();
        int sessionId;

        await using (var context = GetDbContext(dbName))
        {
            var (movieId, hallId) = await CreateDependenciesAsync(context);
            var session = new Session { StartTime = DateTime.UtcNow, MovieId = movieId, HallId = hallId };
            context.Sessions.Add(session);
            await context.SaveChangesAsync();
            sessionId = session.Id;

            context.Orders.Add(new Order 
            { 
                SessionId = sessionId, 
                UserId = "user1"
            });
            await context.SaveChangesAsync();
        }

        await using (var context = GetDbContext(dbName))
        {
            var repo = new SessionRepository(context);
            var result = await repo.HasAnyOrdersAsync(sessionId);

            result.Should().BeTrue();
        }
    }

    [Fact]
    public async Task HasAnyOrdersAsync_ShouldReturnFalse_WhenNoOrders()
    {
        var dbName = Guid.NewGuid().ToString();
        int sessionId;

        await using (var context = GetDbContext(dbName))
        {
            var (movieId, hallId) = await CreateDependenciesAsync(context);
            var session = new Session { StartTime = DateTime.UtcNow, MovieId = movieId, HallId = hallId };
            context.Sessions.Add(session);
            await context.SaveChangesAsync();
            sessionId = session.Id;
        }

        await using (var context = GetDbContext(dbName))
        {
            var repo = new SessionRepository(context);
            var result = await repo.HasAnyOrdersAsync(sessionId);

            result.Should().BeFalse();
        }
    }
}