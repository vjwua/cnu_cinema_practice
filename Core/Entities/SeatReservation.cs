namespace Core.Entities;

using Core.Enums;

public class SeatReservation
{
    public int Id { get; set; }

    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public int SeatId { get; set; }
    public Seat Seat { get; set; } = null!;

    public ReservationStatus Status { get; set; }
    public decimal Price { get; set; }

    // Для тимчасового бронювання
    public DateTime? ReservedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? ReservedByUserId { get; set; }

    // Зв'язок з квитком
    public int? TicketId { get; set; }
    public Ticket? Ticket { get; set; }
}