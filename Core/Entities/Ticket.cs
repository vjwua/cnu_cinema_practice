namespace Core.Entities;

public class Ticket
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    // ❌ ВИДАЛИТИ: public int SeatId { get; set; }
    // ❌ ВИДАЛИТИ: public Seat Seat { get; set; }

    // ✅ ДОДАТИ:
    public int SeatReservationId { get; set; }
    public SeatReservation SeatReservation { get; set; } = null!;

    public decimal Price { get; set; }
    
    public string? QrCodeBase64 { get; set; }
    public DateTime? ScannedAt { get; set; }
    public string? ScannedBy { get; set; }
}