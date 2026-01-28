namespace cnu_cinema_practice.ViewModels.Home;

/// <summary>
/// Session time for display on movie card hover
/// </summary>
public class MovieSessionTimeViewModel
{
    public int SessionId { get; set; }
    public DateTime StartTime { get; set; }
    
    public string FormattedTime => StartTime.ToString("HH:mm");
}
