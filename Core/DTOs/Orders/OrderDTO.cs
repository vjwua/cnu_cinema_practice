using Core.DTOs.Payments;
using Core.Enums;

namespace Core.DTOs.Orders;

public class OrderDTO
{
    public int Id { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string HallName { get; set; } = string.Empty;
    public DateTime SessionStart { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<TicketDTO> Tickets { get; set; } = new List<TicketDTO>();

    public PaymentDTO? Payment { get; set; } 
}