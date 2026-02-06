namespace Core.DTOs.External;

public class ExternalMovieDetailDTO
{
    public required int ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ushort DurationMinutes { get; set; }
    public byte AgeLimit { get; set; }
    public string GenresText { get; set; } = string.Empty;
    public DateOnly? ReleaseDate { get; set; }
    public decimal? ImdbRating { get; set; }
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public string? Country { get; set; }

    public List<string> Directors { get; set; } = new();
    public List<string> Actors { get; set; } = new();
}