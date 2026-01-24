namespace cnu_cinema_practice.ViewModels.Sessions;

public class SessionDetailsViewModel
{
    public int Id { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int MovieDurationMinutes { get; set; }
    public string HallName { get; set; } = string.Empty;
    public int HallId { get; set; }
    public DateTime StartTime { get; set; }
    public decimal BasePrice { get; set; }
    public string MovieFormat { get; set; } = string.Empty;
    public List<SessionSeatViewModel> Seats { get; set; } = [];

    public DateTime EndTime => StartTime.AddMinutes(MovieDurationMinutes);
    public int AvailableSeatsCount => Seats.Count(s => s.IsAvailable);
    public int TotalSeats => Seats.Count;
}