namespace cnu_cinema_practice.ViewModels
{
    public class CheckoutViewModel
    {
        public int MovieId { get; set; }
        public string MovieTitle { get; set; }
        public string PosterUrl { get; set; }
        public DateTime ShowDateTime { get; set; }
        public string Theater { get; set; }
        public List<string> SelectedSeats { get; set; }
        public decimal TicketPrice { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal Total => (SelectedSeats.Count * TicketPrice) + ServiceFee;

        // Customer Information
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public string FormattedShowTime => ShowDateTime.ToString("MMMM dd, yyyy 'at' h:mm tt");
        public string FormattedSeats => string.Join(", ", SelectedSeats);
    }
}
