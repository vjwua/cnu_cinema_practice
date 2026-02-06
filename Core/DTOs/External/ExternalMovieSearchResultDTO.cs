namespace Core.DTOs.External;

public class ExternalMovieSearchResultDTO
{
    public required int ExternalId { get; set; }
    public required string Name { get; set; }
    public string? PosterUrl { get; set; }
    public DateOnly? ReleaseDate { get; set; }
}