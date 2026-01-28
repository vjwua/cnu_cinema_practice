namespace cnu_cinema_practice.ViewModels.Sessions;

public class SessionSeatViewModel
{
    public int SeatId { get; set; }
    public byte RowNum { get; set; }
    public byte SeatNum { get; set; }
    public string SeatType { get; set; } = string.Empty;
    public decimal AddedPrice { get; set; }
    public bool IsAvailable { get; set; }
    public decimal TotalPrice { get; set; }
}