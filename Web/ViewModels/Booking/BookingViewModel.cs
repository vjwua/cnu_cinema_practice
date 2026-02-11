using Core.DTOs.Halls;
using Core.DTOs.Seats;
using Core.Entities;

namespace cnu_cinema_practice.ViewModels
{
    public class BookingViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PosterUrl { get; set; }
        public int ShowtimeId { get; set; }
        public DateTime ShowDateTime { get; set; }
        public HallDetailDTO HallData { get; set; }
        public int HallId { get; set; }
        public decimal BasePrice { get; set; }
        public List<ShowtimeOption> AvailableShowtimes { get; set; }
        public IEnumerable<SeatDTO> SeatLayout { get; set; }
        public byte[][] LayoutArray { get; set; }
        public string alertMessage { get; set; } = "";
        public decimal[] addedPrice { get; set; }
    }

    public class ShowtimeOption
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public string Hall { get; set; }
        public string FormattedTime => DateTime.ToString("h:mm tt");
    }

    public class SeatLayout
    {
        public int Rows { get; set; }
        public int SeatsPerRow { get; set; }
        //public List<string> OccupiedSeats { get; set; } // e.g., ["A1", "A2", "B5"]
        public IEnumerable<SeatDTO> AvailableSeats { get; set; }
    }
}
