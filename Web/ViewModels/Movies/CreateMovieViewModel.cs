namespace cnu_cinema_practice.ViewModels.Movies
{
    public class CreateMovieViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PosterUrl { get; set; }
        public string TrailerUrl { get; set; }
        public int DurationMinutes { get; set; }
        public decimal? ImdbRating { get; set; }
        public byte AgeLimit { get; set; }
        public DateOnly ReleaseDate { get; set; }
        public string Directors { get; set; } // Comma-separated
        public string Actors { get; set; } // Comma-separated
        public string Description { get; set; }
        public string GenresString { get; set; } // Comma-separated
        public string Country { get; set; }
    }
}
