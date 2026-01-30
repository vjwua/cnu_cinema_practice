using Core.DTOs.Sessions;
using Core.Enums;

namespace Core.DTOs.Movies;

public class MovieWithSessionsDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? PosterUrl { get; set; }
    public decimal? ImdbRating { get; set; }
    public MovieGenre Genre { get; set; }
    public ushort DurationMinutes { get; set; }
    public string? Description { get; set; }
    public IEnumerable<SessionListDTO> Sessions { get; set; } = new List<SessionListDTO>();
}
