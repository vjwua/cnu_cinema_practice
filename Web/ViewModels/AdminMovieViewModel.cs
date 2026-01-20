namespace cnu_cinema_practice.ViewModels
{
    public class AdminMovieViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PosterUrl { get; set; }
        public int DurationMinutes { get; set; }
        public string ImdbRating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Director { get; set; }
        public string Description { get; set; }
        public List<string> Genres { get; set; }
        public bool IsActive { get; set; }
    }

    public class MovieFormViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PosterUrl { get; set; }
        public int DurationMinutes { get; set; }
        public string ImdbRating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Director { get; set; }
        public string Description { get; set; }
        public string GenresString { get; set; } // Comma-separated
        public bool IsActive { get; set; }
    }
}
