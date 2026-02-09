using AutoMapper;
using Core.DTOs.Orders;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Services;
using FluentAssertions;
using Moq;

namespace Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<ISeatReservationRepository> _reservationRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _reservationRepoMock = new Mock<ISeatReservationRepository>();
        _mapperMock = new Mock<IMapper>();

        _service = new OrderService(
            _orderRepoMock.Object,
            _reservationRepoMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldCreateOrder_WhenValidationPasses()
    {
        var userId = "user1";
        var dto = new CreateOrderDTO { SeatReservationIds = new List<int> { 1, 2 } };

        var reservations = new List<SeatReservation>
        {
            new SeatReservation 
            { 
                Id = 1, 
                ReservedByUserId = userId, 
                Status = ReservationStatus.Reserved, 
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                SessionId = 100,
                Price = 100
            },
            new SeatReservation 
            { 
                Id = 2, 
                ReservedByUserId = userId, 
                Status = ReservationStatus.Reserved, 
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                SessionId = 100,
                Price = 100
            }
        };

        _reservationRepoMock
            .Setup(repo => repo.GetByIdsAsync(dto.SeatReservationIds))
            .ReturnsAsync(reservations);

        _orderRepoMock
            .Setup(repo => repo.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => 
            {
                o.Id = 1;
                return o;
            });

        _mapperMock
            .Setup(m => m.Map<OrderDTO>(It.IsAny<Order>()))
            .Returns(new OrderDTO { Id = 1, Status = "Pending" });

        var result = await _service.CreateOrderAsync(userId, dto);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);

        _orderRepoMock.Verify(repo => repo.CreateAsync(It.Is<Order>(o => 
            o.UserId == userId && 
            o.Status == OrderStatus.Pending &&
            o.Tickets.Count == 2
        )), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldThrow_WhenReservationNotFound()
    {
        var userId = "user1";
        var dto = new CreateOrderDTO { SeatReservationIds = new List<int> { 1, 2 } };

        var reservations = new List<SeatReservation>
        {
            new SeatReservation { Id = 1 }
        };

        _reservationRepoMock
            .Setup(repo => repo.GetByIdsAsync(dto.SeatReservationIds))
            .ReturnsAsync(reservations);

        var act = async () => await _service.CreateOrderAsync(userId, dto);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("One or more reservations not found.");
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldThrow_WhenUserDoesNotOwnReservation()
    {
        var userId = "user1";
        var otherUser = "user2";
        var dto = new CreateOrderDTO { SeatReservationIds = new List<int> { 1 } };

        var reservations = new List<SeatReservation>
        {
            new SeatReservation { Id = 1, ReservedByUserId = otherUser }
        };

        _reservationRepoMock
            .Setup(repo => repo.GetByIdsAsync(dto.SeatReservationIds))
            .ReturnsAsync(reservations);

        var act = async () => await _service.CreateOrderAsync(userId, dto);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You can only create orders for your own reservations.");
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldThrow_WhenReservationIsExpired()
    {
        var userId = "user1";
        var dto = new CreateOrderDTO { SeatReservationIds = new List<int> { 1 } };

        var reservations = new List<SeatReservation>
        {
            new SeatReservation 
            { 
                Id = 1, 
                ReservedByUserId = userId,
                Status = ReservationStatus.Reserved,
                ExpiresAt = DateTime.UtcNow.AddMinutes(-5)
            }
        };

        _reservationRepoMock
            .Setup(repo => repo.GetByIdsAsync(dto.SeatReservationIds))
            .ReturnsAsync(reservations);

        var act = async () => await _service.CreateOrderAsync(userId, dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Some reservations are expired or already sold.");
    }
    
    [Fact]
    public async Task CreateOrderAsync_ShouldThrow_WhenReservationIsSold()
    {
        var userId = "user1";
        var dto = new CreateOrderDTO { SeatReservationIds = new List<int> { 1 } };

        var reservations = new List<SeatReservation>
        {
            new SeatReservation 
            { 
                Id = 1, 
                ReservedByUserId = userId,
                Status = ReservationStatus.Sold,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            }
        };

        _reservationRepoMock
            .Setup(repo => repo.GetByIdsAsync(dto.SeatReservationIds))
            .ReturnsAsync(reservations);

        var act = async () => await _service.CreateOrderAsync(userId, dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Some reservations are expired or already sold.");
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldThrow_WhenSessionsAreDifferent()
    {
        var userId = "user1";
        var dto = new CreateOrderDTO { SeatReservationIds = new List<int> { 1, 2 } };

        var reservations = new List<SeatReservation>
        {
            new SeatReservation 
            { 
                Id = 1, ReservedByUserId = userId, Status = ReservationStatus.Reserved, 
                ExpiresAt = DateTime.UtcNow.AddMinutes(10), 
                SessionId = 100 
            },
            new SeatReservation 
            { 
                Id = 2, ReservedByUserId = userId, Status = ReservationStatus.Reserved, 
                ExpiresAt = DateTime.UtcNow.AddMinutes(10), 
                SessionId = 200
            }
        };

        _reservationRepoMock
            .Setup(repo => repo.GetByIdsAsync(dto.SeatReservationIds))
            .ReturnsAsync(reservations);

        var act = async () => await _service.CreateOrderAsync(userId, dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("All tickets must be for the same session.");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenExists()
    {
        var orderId = 1;
        var order = new Order { Id = orderId };
        
        _orderRepoMock.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
        _mapperMock.Setup(m => m.Map<OrderDTO>(order)).Returns(new OrderDTO { Id = orderId });

        var result = await _service.GetByIdAsync(orderId);

        result.Should().NotBeNull();
        result.Id.Should().Be(orderId);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        _orderRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Order?)null);

        var result = await _service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserOrdersAsync_ShouldReturnOrders_WhenExist()
    {
        var userId = "u1";
        var orders = new List<Order> { new Order(), new Order() };
        
        _orderRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(orders);
        _mapperMock.Setup(m => m.Map<IEnumerable<OrderDTO>>(orders))
                   .Returns(new List<OrderDTO> { new OrderDTO(), new OrderDTO() });

        var result = await _service.GetUserOrdersAsync(userId);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserOrdersAsync_ShouldReturnEmpty_WhenNoneExist()
    {
        var userId = "u1";
        var orders = new List<Order>();
        
        _orderRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(orders);
        _mapperMock.Setup(m => m.Map<IEnumerable<OrderDTO>>(orders))
                   .Returns(new List<OrderDTO>());

        var result = await _service.GetUserOrdersAsync(userId);

        result.Should().BeEmpty();
    }
}