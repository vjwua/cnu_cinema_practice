using Core.Enums;

namespace Core.Entities;

public class Movie
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public byte DurationMinutes { get; set; }
    public byte AgeLimit { get; set; }
    public MovieGenre Genre { get; set; }

    public string? Description { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public decimal? ImdbRating { get; set; }

    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public string? Director { get; set; }
    public string? Country { get; set; }

    public ICollection<Session> Sessions { get; set; } = new List<Session>();

    public List<Person> Actors { get; } = new List<Person>();
    public List<Person> Directors { get; } = new List<Person>();
}
