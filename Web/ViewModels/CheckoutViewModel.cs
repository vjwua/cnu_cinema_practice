namespace cnu_cinema_practice.ViewModels
{
    public class CheckoutViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PosterUrl { get; set; }
        public DateTime ShowDateTime { get; set; }
        public string Hall { get; set; }
        public List<string> SelectedSeats { get; set; }
        public List<int> ReservationIds { get; set; } = new();
        public decimal BasePrice { get; set; }
        public decimal Total { get; set; }



        public string FormattedShowTime => ShowDateTime.ToString("MMMM dd, yyyy 'at' h:mm tt");
        public string FormattedSeats => string.Join(", ", SelectedSeats);

        public List<SeatCheckoutItem> SeatDetails { get; set; } = new();
    }

    public class SeatCheckoutItem
    {
        public string SeatNumber { get; set; }
        public string Type { get; set; }
        public decimal Price { get; set; }
    }
}
