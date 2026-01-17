using Core.Enums;

namespace Core.Entities;

public class Seat
{
    public int Id { get; set; }

    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public byte RowNum { get; set; }
    public byte SeatNum { get; set; }

    public SeatType SeatType { get; set; }
    public decimal AddedPrice { get; set; }

    public bool IsAvailable { get; set; }
}
