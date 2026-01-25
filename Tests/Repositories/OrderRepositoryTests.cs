using Core.Entities;
using Core.Enums;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace Tests.Repositories;

public class OrderRepositoryTests
{
    private readonly DbContextOptions<CinemaDbContext> _dbOptions;

    public OrderRepositoryTests()
    {
        _dbOptions = new DbContextOptionsBuilder<CinemaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private CinemaDbContext CreateContext() => new CinemaDbContext(_dbOptions);

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenOrderExists()
    {
        await using var context = CreateContext();

        var seatType = new SeatType
        {
            Name = "Standard",
            AddedPrice = 0,
            ColorCode = "#FFFFFF"
        };
        
        var hall = new Hall("Red", 10, 10); 

        var movie = new Movie
        {
            Name = "Dune",
            DurationMinutes = 120,
            Description = "Epic sci-fi",
            PosterUrl = "url",
            TrailerUrl = "url",
            Country = "USA",
            ReleaseDate = DateOnly.FromDateTime(DateTime.Now),
            ImdbRating = 8.5m,
            AgeLimit = 12,
            Genre = MovieGenre.SciFi
        };

        await context.SeatTypes.AddAsync(seatType);
        await context.Halls.AddAsync(hall);
        await context.Movies.AddAsync(movie);
        await context.SaveChangesAsync();

        var session = new Session
        {
            MovieId = movie.Id,
            HallId = hall.Id,
            StartTime = DateTime.UtcNow,
            BasePrice = 100m,
            MovieFormat = MovieFormat.TwoD
        };

        var seat = new Seat
        {
            HallId = hall.Id,
            RowNum = 5,
            SeatNum = 10,
            SeatTypeId = seatType.Id
        };

        await context.Sessions.AddAsync(session);
        await context.Seats.AddAsync(seat);
        await context.SaveChangesAsync();

        var reservation = new SeatReservation
        {
            SeatId = seat.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Status = ReservationStatus.Reserved
        };

        var ticket = new Ticket
        {
            SeatReservation = reservation,
            Price = 100
        };

        var order = new Order
        {
            UserId = "user1",
            Status = OrderStatus.Created,
            SessionId = session.Id,
            CreatedAt = DateTime.UtcNow
        };
        
        order.Tickets.Add(ticket);

        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var repository = new OrderRepository(CreateContext());
        var result = await repository.GetByIdAsync(order.Id);

        result.Should().NotBeNull();
        
        result.Id.Should().Be(order.Id);
        
        result.Session.Should().NotBeNull();
        result.Session.Movie.Name.Should().Be("Dune");
        
        result.Tickets.Should().HaveCount(1);
        result.Tickets.First().SeatReservation.Seat.RowNum.Should().Be(5);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenOrderDoesNotExist()
    {
        await using var context = CreateContext();
        var repository = new OrderRepository(context);

        var result = await repository.GetByIdAsync(9999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnOrdersForSpecificUser_SortedByDateDesc()
    {
        await using var context = CreateContext();
        var userId = "target_user";

        var hall = new Hall("Test", 5, 5);
        var movie = new Movie { Name = "M", Genre = MovieGenre.Action };
        await context.Halls.AddAsync(hall);
        await context.Movies.AddAsync(movie);
        await context.SaveChangesAsync();

        var session = new Session { MovieId = movie.Id, HallId = hall.Id, StartTime = DateTime.UtcNow };
        await context.Sessions.AddAsync(session);
        await context.SaveChangesAsync();

        var orderOld = new Order
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            SessionId = session.Id,
            Status = OrderStatus.Paid
        };
        var orderNew = new Order
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            SessionId = session.Id,
            Status = OrderStatus.Paid
        };
        var orderOther = new Order
        {
            UserId = "other",
            CreatedAt = DateTime.UtcNow,
            SessionId = session.Id,
            Status = OrderStatus.Paid
        };

        await context.Orders.AddRangeAsync(orderOld, orderNew, orderOther);
        await context.SaveChangesAsync();

        var repository = new OrderRepository(CreateContext());

        var result = (await repository.GetByUserIdAsync(userId)).ToList();

        result.Should().HaveCount(2);
        result.First().Id.Should().Be(orderNew.Id);
        result.Last().Id.Should().Be(orderOld.Id);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddOrderToDatabase()
    {
        var repository = new OrderRepository(CreateContext());
        
        var newOrder = new Order
        {
            UserId = "user1",
            Status = OrderStatus.Created,
            SessionId = 1, 
            CreatedAt = DateTime.UtcNow
        };

        var createdOrder = await repository.CreateAsync(newOrder);

        createdOrder.Id.Should().BeGreaterThan(0);
        
        await using var verifyContext = CreateContext();
        var savedOrder = await verifyContext.Orders.FirstOrDefaultAsync(o => o.UserId == "user1");

        savedOrder.Should().NotBeNull();
        savedOrder.Status.Should().Be(OrderStatus.Created);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateOrderInDatabase()
    {
        await using var context = CreateContext();
        var order = new Order
        {
            UserId = "user1",
            Status = OrderStatus.Pending,
            SessionId = 1,
            CreatedAt = DateTime.UtcNow
        };
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var repository = new OrderRepository(CreateContext());

        order.Status = OrderStatus.Paid;
        await repository.UpdateAsync(order);

        await using var verifyContext = CreateContext();
        var updatedOrder = await verifyContext.Orders.FindAsync(order.Id);
        updatedOrder!.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public async Task GetExpiredPendingOrdersAsync_ShouldReturnOnlyExpiredPendingOrders()
    {
        await using var context = CreateContext();
        var cutoffTime = DateTime.UtcNow.AddMinutes(-15);

        var expiredOrder = new Order
        {
            UserId = "user1",
            Status = OrderStatus.Pending,
            CreatedAt = cutoffTime.AddMinutes(-5),
            SessionId = 1
        };
        var freshOrder = new Order
        {
            UserId = "user2",
            Status = OrderStatus.Pending,
            CreatedAt = cutoffTime.AddMinutes(5),
            SessionId = 1
        };
        var paidOrder = new Order
        {
            UserId = "user3",
            Status = OrderStatus.Paid,
            CreatedAt = cutoffTime.AddMinutes(-5),
            SessionId = 1
        };

        await context.Orders.AddRangeAsync(expiredOrder, freshOrder, paidOrder);
        await context.SaveChangesAsync();

        var repository = new OrderRepository(CreateContext());

        var result = (await repository.GetExpiredPendingOrdersAsync(cutoffTime)).ToList();

        result.Should().HaveCount(1);
        result.Single().Id.Should().Be(expiredOrder.Id);
    }

    [Fact]
    public async Task GetOrdersBySessionIdAsync_ShouldReturnOrdersForSession()
    {
        await using var context = CreateContext();
        var targetSessionId = 10;
        var otherSessionId = 999;

        await context.Orders.AddRangeAsync(
            new Order { SessionId = targetSessionId, UserId = "u1", Status = OrderStatus.Paid },
            new Order { SessionId = targetSessionId, UserId = "u2", Status = OrderStatus.Paid },
            new Order { SessionId = otherSessionId, UserId = "u3", Status = OrderStatus.Paid }
        );
        await context.SaveChangesAsync();

        var repository = new OrderRepository(CreateContext());

        var result = (await repository.GetOrdersBySessionIdAsync(targetSessionId)).ToList();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(o => o.SessionId == targetSessionId);
    }
    
    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenUserHasNoOrders()
    {
        await using var context = CreateContext();
        var repository = new OrderRepository(context);

        var result = (await repository.GetByUserIdAsync("ghost_user")).ToList();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetExpiredPendingOrdersAsync_ShouldReturnEmpty_WhenNoExpiredOrders()
    {
        await using var context = CreateContext();
        var repository = new OrderRepository(context);
        
        var result = (await repository.GetExpiredPendingOrdersAsync(DateTime.UtcNow)).ToList();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOrdersBySessionIdAsync_ShouldReturnEmpty_WhenSessionHasNoOrders()
    {
        await using var context = CreateContext();
        var repository = new OrderRepository(context);

        var result = (await repository.GetOrdersBySessionIdAsync(9999)).ToList();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}