using Core.Entities;
using Core.Enums;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace Tests.Repositories;

public class SeatReservationRepositoryTests
{
    private readonly DbContextOptions<CinemaDbContext> _dbOptions;

    public SeatReservationRepositoryTests()
    {
        _dbOptions = new DbContextOptionsBuilder<CinemaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private CinemaDbContext CreateContext() => new CinemaDbContext(_dbOptions);

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnReservationsWithSeats_WhenIdsExist()
    {
        await using var context = CreateContext();

        var seat1 = new Seat { Id = 1, RowNum = 1, SeatNum = 1, SeatTypeId = 1, HallId = 1 };
        var seat2 = new Seat { Id = 2, RowNum = 1, SeatNum = 2, SeatTypeId = 1, HallId = 1 };

        var reservation1 = new SeatReservation { Id = 1, SeatId = 1, Seat = seat1, Status = ReservationStatus.Reserved };
        var reservation2 = new SeatReservation { Id = 2, SeatId = 2, Seat = seat2, Status = ReservationStatus.Reserved };
        var reservation3 = new SeatReservation { Id = 3, SeatId = 2, Seat = seat2, Status = ReservationStatus.Reserved };

        await context.Seats.AddRangeAsync(seat1, seat2);
        await context.SeatReservations.AddRangeAsync(reservation1, reservation2, reservation3);
        await context.SaveChangesAsync();

        var repository = new SeatReservationRepository(CreateContext());

        var idsToFind = new List<int> { 1, 2 };
        var result = await repository.GetByIdsAsync(idsToFind);

        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Id == 1);
        result.Should().Contain(r => r.Id == 2);
        result.Should().NotContain(r => r.Id == 3);

        result.First(r => r.Id == 1).Seat.Should().NotBeNull();
        result.First(r => r.Id == 1).Seat.SeatNum.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnEmptyList_WhenIdsDoNotExist()
    {
        await using var context = CreateContext();
        var repository = new SeatReservationRepository(context);

        var result = await repository.GetByIdsAsync(new List<int> { 99, 100 });

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnEmptyList_WhenInputListIsEmpty()
    {
        await using var context = CreateContext();
        var repository = new SeatReservationRepository(context);

        var result = await repository.GetByIdsAsync(new List<int>());

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task MarkAsSoldAsync_ShouldUpdateStatusToSold_WhenReservationsExist()
    {
        await using var context = CreateContext();
        var reservation = new SeatReservation { Id = 1, Status = ReservationStatus.Reserved, SeatId = 1 };
        await context.SeatReservations.AddAsync(reservation);
        await context.SaveChangesAsync();

        var repository = new SeatReservationRepository(CreateContext());

        await repository.MarkAsSoldAsync(new List<int> { 1 });

        await using var verifyContext = CreateContext();
        var updatedReservation = await verifyContext.SeatReservations.FindAsync(1);
        
        updatedReservation.Should().NotBeNull();
        updatedReservation.Status.Should().Be(ReservationStatus.Sold);
    }

    [Fact]
    public async Task MarkAsSoldAsync_ShouldDoNothing_WhenIdsDoNotExist()
    {
        await using var context = CreateContext();
        var reservation = new SeatReservation { Id = 1, Status = ReservationStatus.Reserved, SeatId = 1 };
        await context.SeatReservations.AddAsync(reservation);
        await context.SaveChangesAsync();

        var repository = new SeatReservationRepository(CreateContext());

        await repository.MarkAsSoldAsync(new List<int> { 99 });

        await using var verifyContext = CreateContext();
        var existingReservation = await verifyContext.SeatReservations.FindAsync(1);
        
        existingReservation!.Status.Should().Be(ReservationStatus.Reserved);
    }
}