namespace cnu_cinema_practice.ViewModels
{
    public class BookingViewModel
    {
        public int MovieId { get; set; }
        public string MovieTitle { get; set; }
        public string PosterUrl { get; set; }
        public int ShowtimeId { get; set; }
        public DateTime ShowDateTime { get; set; }
        public string Theater { get; set; }
        public decimal TicketPrice { get; set; }
        public List<ShowtimeOption> AvailableShowtimes { get; set; }
        public SeatLayout SeatLayout { get; set; }
    }

    public class ShowtimeOption
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public string Theater { get; set; }
        public string FormattedTime => DateTime.ToString("h:mm tt");
    }

    public class SeatLayout
    {
        public int Rows { get; set; }
        public int SeatsPerRow { get; set; }
        public List<string> OccupiedSeats { get; set; } // e.g., ["A1", "A2", "B5"]
    }
}
