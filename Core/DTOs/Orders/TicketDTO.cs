namespace Core.DTOs.Orders;

public class TicketDTO
{
    public int Id { get; set; }

    public int RowNum { get; set; }
    public int SeatNum { get; set; }
    public string SeatType { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public string? QrCode { get; set; }
}