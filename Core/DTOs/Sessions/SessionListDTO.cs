using Core.Enums;

namespace Core.DTOs.Sessions;

public class SessionListDTO
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieName { get; set; } = string.Empty;
    public string? MoviePosterUrl { get; set; }
    public int HallId { get; set; }
    public string HallName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public decimal BasePrice { get; set; }
    public MovieFormat MovieFormat { get; set; }
    public int MovieDurationMinutes { get; set; }
    public int TotalSeats { get; set; }
    public int OccupiedSeats { get; set; }
}
