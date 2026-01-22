namespace Core.DTOs.Seats;

public class SeatDTO
{
    public int Id { get; set; }
    public int HallId { get; set; }
    public byte RowNum { get; set; }
    public byte SeatNum { get; set; }
    public int SeatTypeId { get; set; }
}