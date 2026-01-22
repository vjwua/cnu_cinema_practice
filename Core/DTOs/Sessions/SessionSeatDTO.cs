namespace Core.DTOs.Sessions;

public class SessionSeatDTO
{
    public int SeatId { get; set; }

    public byte RowNum { get; set; }
    public byte SeatNum { get; set; }

    public string SeatType { get; set; } = string.Empty;
    public decimal AddedPrice { get; set; }

    public bool IsAvailable { get; set; }
}