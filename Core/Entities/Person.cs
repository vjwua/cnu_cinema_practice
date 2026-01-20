namespace Core.Entities;

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = null!; // Виправлено: додано null!

    public List<Movie> ActedMovies { get; private set; } = new List<Movie>();
    public List<Movie> DirectedMovies { get; private set; } = new List<Movie>();
}