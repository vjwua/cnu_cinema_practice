namespace Core.Entities;

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }

    public List<Movie> ActedMovies { get; set; } = new List<Movie>();
    public List<Movie> DirectedMovies { get; set; } = new List<Movie>();
}