using Core.Enums;

namespace cnu_cinema_practice.ViewModels.Account;

public class OrderViewModel
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }

    public string MovieName { get; set; } = null!;
    public string? MoviePosterUrl { get; set; }
    public DateTime SessionStartTime { get; set; }
    public string HallName { get; set; } = null!;

    public List<TicketViewModel> Tickets { get; set; } = new();

    public bool IsPaid { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? PaidAt { get; set; }
}