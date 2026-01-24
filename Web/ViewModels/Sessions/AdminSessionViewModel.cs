namespace cnu_cinema_practice.ViewModels.Sessions;

public class AdminSessionViewModel
{
    public int Id { get; set; }
    public string MovieName { get; set; } = string.Empty;
    public string? MoviePosterUrl { get; set; }
    public string HallName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public decimal BasePrice { get; set; }
    public string MovieFormat { get; set; } = string.Empty;
}