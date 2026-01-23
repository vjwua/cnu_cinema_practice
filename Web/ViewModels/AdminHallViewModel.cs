namespace cnu_cinema_practice.ViewModels
{
    public class AdminHallViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Rows { get; set; }
        public int SeatsPerRow { get; set; }
        public int TotalSeats => Rows * SeatsPerRow;
        public int AddedPrice { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class HallFormViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Rows { get; set; }
        public int SeatsPerRow { get; set; }
        public bool IsActive { get; set; }
    }

    public class HallLayoutViewModel
    {
        public int HallId { get; set; }
        public string HallName { get; set; }
        public int Rows { get; set; }
        public int SeatsPerRow { get; set; }
        public List<string> DisabledSeats { get; set; } // Seats that are broken/removed
    }
}
