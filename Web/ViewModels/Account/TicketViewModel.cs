namespace cnu_cinema_practice.ViewModels.Account;

public class TicketViewModel
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public string RowNum { get; set; } = string.Empty;
    public int SeatNum { get; set; }
    public string SeatTypeName { get; set; } = null!;
    public string? QrCode { get; set; }
}