namespace cnu_cinema_practice.ViewModels.Halls;

public class SeatViewModel
{
    public int Id { get; set; }
    public int HallId { get; set; }
    public byte RowNum { get; set; }
    public byte SeatNum { get; set; }
    public int SeatTypeId { get; set; }
}