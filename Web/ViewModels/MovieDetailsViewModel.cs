using Core.Entities;

namespace cnu_cinema_practice.ViewModels;

public class MovieDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string PosterUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty; // Converted from Enum
    public string Duration { get; set; } = string.Empty;
    public string Rating { get; set; } = string.Empty; // e.g., "9.1"
    public string AgeLimit { get; set; } = string.Empty; // e.g., "12+"
    public List<string> Actors { get; set; } = new();
    public List<string> Directors { get; set; } = new();

    // Sessions Logic
    public bool HasNoSessions { get; set; }
    public WeekSessionsViewModel WeekSessions { get; set; } = new();
}

public class WeekSessionsViewModel
{
    public List<DaySessionsViewModel> SessionsByDate { get; set; } = new();
}

public class DaySessionsViewModel
{
    public DateTime Date { get; set; }
    public string DisplayDate { get; set; } = string.Empty; // "СЬОГОДНІ", "ЗАВТРА" or date
    public string TabId { get; set; } = string.Empty; // "day-1", "day-2" etc.
    public List<SessionCardViewModel> Sessions { get; set; } = new();
}

public class SessionCardViewModel
{
    public int Id { get; set; }
    public string Time { get; set; } = string.Empty; // "12:00"
    public string FormatAndHall { get; set; } = string.Empty; // "IMAX LASER • ЗАЛ 1"
    public string Price { get; set; } = string.Empty;
    public bool IsVIP { get; set; }
}
