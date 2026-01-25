using Core.Enums;

namespace Core.DTOs.Payments;

public class CreatePaymentDTO
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    
    // Якщо буде реальна платіжна система (Stripe/LiqPay), 
    // тут ще буде поле типу "PaymentToken" або "TransactionId"
}