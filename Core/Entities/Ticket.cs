namespace Core.Entities;

public class Ticket
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int SeatId { get; set; }
    public Seat Seat { get; set; } = null!;

    public decimal TotalPrice { get; set; }
}
