using AutoMapper;
using Core.DTOs.Payments;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Core.Services;

public class PaymentService(
    IPaymentRepository paymentRepository,
    IOrderRepository orderRepository,
    ISeatReservationRepository reservationRepository,
    IMapper mapper,
    IUnitOfWork unitOfWork) : IPaymentService
{
    public async Task<PaymentDTO> ProcessPaymentAsync(CreatePaymentDTO dto)
    {
        var order = await orderRepository.GetByIdAsync(dto.OrderId);
        
        if (order == null)
            throw new KeyNotFoundException("Order not found.");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Order status is {order.Status}, payment cannot be processed.");

        var expectedAmount = order.Tickets.Sum(t => t.Price);
        if (dto.Amount != expectedAmount)
        {
            throw new ArgumentException($"Incorrect amount. Expected: {expectedAmount}, but received: {dto.Amount}");
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod,
                PaidAt = DateTime.UtcNow
            };

            await paymentRepository.CreateAsync(payment);

            order.Status = OrderStatus.Paid;
            await orderRepository.UpdateAsync(order);

            if (order.Tickets.Any())
            {
                var reservationIds = order.Tickets.Select(t => t.SeatReservationId).ToList();
                await reservationRepository.MarkAsSoldAsync(reservationIds);
            }

            await unitOfWork.CommitTransactionAsync();

            return mapper.Map<PaymentDTO>(payment);
        }
        catch (Exception)
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}