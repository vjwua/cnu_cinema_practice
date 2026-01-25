using cnu_cinema_practice.ViewModels.Halls;

namespace cnu_cinema_practice.ViewModels.Sessions;

public class SessionScheduleViewModel
{
    public DateTime SelectedDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public IEnumerable<AdminSessionViewModel> Sessions { get; set; } = [];
    public IEnumerable<HallListViewModel> Halls { get; set; } = [];

    public IEnumerable<AdminSessionViewModel> GetSessionsForHallAndDate(int hallId, DateTime date)
    {
        return Sessions
            .Where(s => s.HallName == Halls.FirstOrDefault(h => h.Id == hallId)?.Name
                        && s.StartTime.Date == date.Date)
            .OrderBy(s => s.StartTime);
    }

    public IEnumerable<DateTime> GetDatesInRange()
    {
        for (var date = StartDate; date < EndDate; date = date.AddDays(1))
        {
            yield return date;
        }
    }
}