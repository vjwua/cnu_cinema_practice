using Core.Entities;
using Core.Enums;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace Tests.Repositories;

public class PaymentRepositoryTests
{
    private readonly DbContextOptions<CinemaDbContext> _dbOptions;

    public PaymentRepositoryTests()
    {
        _dbOptions = new DbContextOptionsBuilder<CinemaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private CinemaDbContext CreateContext() => new CinemaDbContext(_dbOptions);

    [Fact]
    public async Task GetByOrderIdAsync_ShouldReturnPayment_WhenPaymentExists()
    {
        await using var context = CreateContext();
        
        var order = new Order { Id = 1, UserId = "user1", Status = OrderStatus.Created };
        
        var payment = new Payment
        {
            Id = 1,
            OrderId = order.Id,
            Amount = 200.50m,
            PaymentMethod = PaymentMethod.Card,
            PaidAt = DateTime.UtcNow
        };

        await context.Orders.AddAsync(order);
        await context.Payments.AddAsync(payment);
        await context.SaveChangesAsync();

        var repository = new PaymentRepository(CreateContext());

        var result = await repository.GetByOrderIdAsync(order.Id);

        result.Should().NotBeNull();
        result.OrderId.Should().Be(order.Id);
        result.Amount.Should().Be(200.50m);
        result.PaymentMethod.Should().Be(PaymentMethod.Card);
    }

    [Fact]
    public async Task GetByOrderIdAsync_ShouldReturnNull_WhenPaymentDoesNotExist()
    {
        await using var context = CreateContext();
        var order = new Order { Id = 100, UserId = "user1" };
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var repository = new PaymentRepository(CreateContext());

        var result = await repository.GetByOrderIdAsync(100);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByOrderIdAsync_ShouldReturnNull_WhenOrderIdIsInvalid()
    {
        var repository = new PaymentRepository(CreateContext());

        var result = await repository.GetByOrderIdAsync(-1);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddPaymentToDatabase()
    {
        var repository = new PaymentRepository(CreateContext());
        
        var newPayment = new Payment
        {
            OrderId = 5,
            Amount = 150.00m,
            PaymentMethod = PaymentMethod.ApplePay,
            PaidAt = DateTime.UtcNow
        };

        var result = await repository.CreateAsync(newPayment);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        await using var verifyContext = CreateContext();
        var savedPayment = await verifyContext.Payments.FirstOrDefaultAsync(p => p.OrderId == 5);
        
        savedPayment.Should().NotBeNull();
        savedPayment.Amount.Should().Be(150.00m);
        savedPayment.PaymentMethod.Should().Be(PaymentMethod.ApplePay);
    }
}