namespace Core.DTOs.Sessions;

public class SessionPreviewDTO
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public decimal BasePrice { get; set; }
    public string HallName { get; set; } = string.Empty;
    public string? MoviePosterUrl { get; set; }
    public int MovieDurationMinutes { get; set; }
    public string? MovieGenre { get; set; }
}
