using AutoMapper;
using Core.DTOs.Payments;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Services;
using FluentAssertions;
using Moq;

namespace Tests.Services;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<ISeatReservationRepository> _reservationRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _orderRepoMock = new Mock<IOrderRepository>();
        _reservationRepoMock = new Mock<ISeatReservationRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();

        _service = new PaymentService(
            _paymentRepoMock.Object,
            _orderRepoMock.Object,
            _reservationRepoMock.Object,
            _mapperMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldProcessPayment_WhenDataIsValid()
    {
        var dto = new CreatePaymentDTO { OrderId = 1, Amount = 200, PaymentMethod = PaymentMethod.Card };
        
        var order = new Order 
        { 
            Id = 1, 
            Status = OrderStatus.Pending
        };
        
        order.Tickets.Add(new Ticket { Price = 100, SeatReservationId = 10 });
        order.Tickets.Add(new Ticket { Price = 100, SeatReservationId = 11 });

        _orderRepoMock.Setup(r => r.GetByIdAsync(dto.OrderId)).ReturnsAsync(order);
        
        _mapperMock.Setup(m => m.Map<PaymentDTO>(It.IsAny<Payment>()))
            .Returns(new PaymentDTO { Id = 1, Amount = 200 });

        var result = await _service.ProcessPaymentAsync(dto);

        result.Should().NotBeNull();
        
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Never);

        order.Status.Should().Be(OrderStatus.Paid);
        _orderRepoMock.Verify(r => r.UpdateAsync(order), Times.Once);

        _reservationRepoMock.Verify(r => r.MarkAsSoldAsync(It.Is<List<int>>(ids => 
            ids.Count == 2 && ids.Contains(10) && ids.Contains(11)
        )), Times.Once);

        _paymentRepoMock.Verify(r => r.CreateAsync(It.Is<Payment>(p => 
            p.Amount == 200 && p.OrderId == 1
        )), Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrow_WhenOrderNotFound()
    {
        _orderRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Order?)null);
        var dto = new CreatePaymentDTO { OrderId = 1 };

        var act = async () => await _service.ProcessPaymentAsync(dto);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Order not found.");
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrow_WhenOrderIsNotPending()
    {
        var order = new Order { Id = 1, Status = OrderStatus.Paid };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);
        var dto = new CreatePaymentDTO { OrderId = 1 };

        var act = async () => await _service.ProcessPaymentAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Order status is {OrderStatus.Paid}, payment cannot be processed.");
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrow_WhenAmountIsIncorrect()
    {
        var order = new Order 
        { 
            Id = 1, Status = OrderStatus.Pending
        };
        order.Tickets.Add(new Ticket { Price = 100 });

        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);
        
        var dto = new CreatePaymentDTO { OrderId = 1, Amount = 50 };

        var act = async () => await _service.ProcessPaymentAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Incorrect amount. Expected: 100, but received: 50");
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldRollback_WhenErrorOccursInsideTransaction()
    {
        var dto = new CreatePaymentDTO { OrderId = 1, Amount = 100 };
        var order = new Order 
        { 
            Id = 1, Status = OrderStatus.Pending
        };
        order.Tickets.Add(new Ticket { Price = 100 });

        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        _paymentRepoMock.Setup(r => r.CreateAsync(It.IsAny<Payment>()))
            .ThrowsAsync(new DbException("Database error"));

        var act = async () => await _service.ProcessPaymentAsync(dto);

        await act.Should().ThrowAsync<DbException>().WithMessage("Database error");

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);
    }
}