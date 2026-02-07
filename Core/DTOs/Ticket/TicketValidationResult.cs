namespace Core.DTOs.Ticket;

public class TicketValidationResult
{
    public bool IsValid { get; set; }
    public string? Error { get; set; }
    public int? TicketId { get; set; }
    public int? SessionId { get; set; }
    public bool AlreadyScanned { get; set; }
    public DateTime? ScannedAt { get; set; }
    public string? ScannedBy { get; set; }
    public int? RowNum { get; set; }
    public int? SeatNum { get; set; }
    public string? MovieName { get; set; }
    public string? HallName { get; set; }
    public DateTime? StartTime { get; set; }
}