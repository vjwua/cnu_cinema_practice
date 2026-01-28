namespace cnu_cinema_practice.ViewModels.Sessions;

public class AdminSessionViewModel
{
    public int Id { get; set; }
    public string MovieName { get; set; } = string.Empty;
    public string? MoviePosterUrl { get; set; }
    public string HallName { get; set; } = string.Empty;
    public int HallId { get; set; }
    public DateTime StartTime { get; set; }
    public decimal BasePrice { get; set; }
    public string MovieFormat { get; set; } = string.Empty;
    public int MovieDuration { get; set; }
    public int OccupiedSeats { get; set; }
    public int TotalSeats { get; set; }
    
    public double OccupancyPercentage => TotalSeats > 0 ? (double)OccupiedSeats / TotalSeats * 100 : 0;
    
    public string GetOccupancyColorClass()
    {
        var percentage = OccupancyPercentage;
        if (percentage >= 80) return "session-almost-full"; // Red
        if (percentage >= 30) return "session-filling"; // Yellow
        return "session-available"; // Green
    }
}
