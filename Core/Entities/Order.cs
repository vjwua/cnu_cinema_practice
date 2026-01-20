using Core.Enums;

namespace Core.Entities;

public class Order
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    
    public Payment? Payment { get; set; }

    public ICollection<Ticket> Tickets { get; private set; } = new List<Ticket>();
}