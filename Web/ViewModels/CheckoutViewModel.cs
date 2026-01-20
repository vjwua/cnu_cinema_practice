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
        public decimal BasePrice { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal Total => (SelectedSeats.Count * BasePrice) + ServiceFee;

        // Customer Information
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public string FormattedShowTime => ShowDateTime.ToString("MMMM dd, yyyy 'at' h:mm tt");
        public string FormattedSeats => string.Join(", ", SelectedSeats);
    }
}
