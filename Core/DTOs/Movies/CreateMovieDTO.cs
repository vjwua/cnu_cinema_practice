using Core.Enums;

namespace Core.DTOs.Movies;

public class CreateMovieDTO
{
    public string Name { get; set; } = null!;
    public ushort DurationMinutes { get; set; }
    public byte AgeLimit { get; set; }
    public MovieGenre Genre { get; set; }
    
    public string? Description { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public decimal? ImdbRating { get; set; }
    
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public string? Country { get; set; }

    public List<int> ActorsIds { get; set; } = new();
    public List<int> DirectorsIds { get; set; } = new();
}