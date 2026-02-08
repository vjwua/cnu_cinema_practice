using Core.Enums;

namespace Core.DTOs.Movies;

public class MovieListDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public byte AgeLimit { get; set; }
    public decimal? ImdbRating { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public MovieGenre Genre { get; set; }
    public ushort DurationMinutes { get; set; }
    public string? Description { get; set; }
    public string? Director { get; set; } = "Unknown";
}