using Core.DTOs.Payments;

namespace Core.Interfaces.Services;

public interface IPaymentService
{
    Task<PaymentDTO> ProcessPaymentAsync(CreatePaymentDTO dto);
}