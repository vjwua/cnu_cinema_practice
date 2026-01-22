// 🔄 ОНОВИТИ: Core/Entities/Movie.cs
namespace Core.Entities;

using Core.Enums;

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
    public string? Country { get; set; }

    // ✅ ДОДАТИ: Many-to-Many навігації
    public ICollection<Person> Directors { get; set; } = new List<Person>();
    public ICollection<Person> Actors { get; set; } = new List<Person>();

    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}