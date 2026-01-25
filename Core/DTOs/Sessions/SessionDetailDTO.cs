using Core.Enums;

namespace Core.DTOs.Sessions;

public class SessionDetailDTO
{
    public int Id { get; set; }

    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int MovieDurationMinutes { get; set; }

    public string HallName { get; set; } = string.Empty;
    public int HallId { get; set; }

    public DateTime StartTime { get; set; }
    public decimal BasePrice { get; set; }
    public MovieFormat MovieFormat { get; set; }

    public List<SessionSeatDTO> Seats { get; set; } = [];
}