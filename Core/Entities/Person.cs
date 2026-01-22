// 🔄 ОНОВИТИ: Core/Entities/Person.cs
namespace Core.Entities;

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<Movie> DirectedMovies { get; set; } = new List<Movie>();
    public ICollection<Movie> ActedInMovies { get; set; } = new List<Movie>();
}