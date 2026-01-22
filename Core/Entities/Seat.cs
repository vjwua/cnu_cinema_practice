using Core.Enums;

namespace Core.Entities;

public class Seat
{
    public int Id { get; set; }

    public int HallId { get; set; }
    public Hall Hall { get; set; } = null!;

    public byte RowNum { get; set; }
    public byte SeatNum { get; set; }

    // ❌ ВИДАЛИТИ: public SeatType SeatType { get; set; }
    // ❌ ВИДАЛИТИ: public decimal AddedPrice { get; set; }
    // ❌ ВИДАЛИТИ: public bool IsAvailable { get; set; }

    // ✅ ДОДАТИ:
    public int SeatTypeId { get; set; }
    public SeatType SeatType { get; set; } = null!;

    public ICollection<SeatReservation> Reservations { get; set; } = new List<SeatReservation>();
}
