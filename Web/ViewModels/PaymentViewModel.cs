using Core.Enums;

namespace cnu_cinema_practice.ViewModels
{
    public class PaymentViewModel
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }

        public decimal BasePrice { get; set; }
        public List<PaymentMethod> AvailablePaymentMethods { get; set; } = new();

        public string MovieTitle { get; set; } = string.Empty;
        public string MoviePosterUrl { get; set; } = string.Empty;
        public DateTime ShowDateTime { get; set; }
        public string HallName { get; set; } = string.Empty;
        public List<string> SelectedSeats { get; set; } = new();

        public string? CardNumber { get; set; }
        public string? ExpiryDate { get; set; }
        public string? Cvv { get; set; }
        public string? CardholderName { get; set; }
        public PaymentMethod SelectedPaymentMethod { get; set; }
        public bool AgreeToTerms { get; set; }
    }
}