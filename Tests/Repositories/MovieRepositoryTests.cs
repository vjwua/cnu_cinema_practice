namespace Tests.Repositories;

using Core.Entities;
using Core.Enums;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;


public class MovieRepositoryTests : IDisposable
{
    private readonly CinemaDbContext _context;
    private readonly MovieRepository _repository;

    public MovieRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<CinemaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CinemaDbContext(options);
        _repository = new MovieRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllMovies_OrderedByReleaseDate()
    {
        // Arrange
        var movies = new List<Movie>
        {
            new Movie 
            { 
                Name = "Old Movie", 
                DurationMinutes = 120, 
                AgeLimit = 13, 
                Genre = MovieGenre.Action,
                ReleaseDate = new DateOnly(2020, 1, 1)
            },
            new Movie 
            { 
                Name = "New Movie", 
                DurationMinutes = 130, 
                AgeLimit = 16, 
                Genre = MovieGenre.Drama,
                ReleaseDate = new DateOnly(2024, 1, 1)
            }
        };

        await _context.Movies.AddRangeAsync(movies);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();
        var resultList = result.ToList();

        // Assert
        resultList.Should().HaveCount(2);
        resultList[0].Name.Should().Be("New Movie"); // новіший перший
        resultList[1].Name.Should().Be("Old Movie");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsMovieWithRelations()
    {
        // Arrange
        var actor = new Person { Name = "Tom Hanks" };
        var director = new Person { Name = "Steven Spielberg" };

        var movie = new Movie
        {
            Name = "Test Movie",
            DurationMinutes = 120,
            AgeLimit = 13,
            Genre = MovieGenre.Drama,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        movie.Actors.Add(actor);
        movie.Directors.Add(director);

        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(movie.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Movie");
        result.Actors.Should().HaveCount(1);
        result.Directors.Should().HaveCount(1);
        result.Actors.First().Name.Should().Be("Tom Hanks");
        result.Directors.First().Name.Should().Be("Steven Spielberg");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidMovie_CreatesAndReturnsMovie()
    {
        // Arrange
        var movie = new Movie
        {
            Name = "New Movie",
            DurationMinutes = 150,
            AgeLimit = 18,
            Genre = MovieGenre.Horror,
            ReleaseDate = new DateOnly(2024, 10, 31)
        };

        // Act
        var result = await _repository.CreateAsync(movie);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Movie");

        var savedMovie = await _context.Movies.FindAsync(result.Id);
        savedMovie.Should().NotBeNull();
        savedMovie!.Name.Should().Be("New Movie");
    }

    [Fact]
    public async Task UpdateAsync_ExistingMovie_UpdatesSuccessfully()
    {
        // Arrange
        var movie = new Movie
        {
            Name = "Original Name",
            DurationMinutes = 120,
            AgeLimit = 13,
            Genre = MovieGenre.Action,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();

        // Detach to simulate a new context
        _context.Entry(movie).State = EntityState.Detached;

        // Modify
        movie.Name = "Updated Name";
        movie.DurationMinutes = 140;

        // Act
        await _repository.UpdateAsync(movie);

        // Assert
        var updatedMovie = await _context.Movies.FindAsync(movie.Id);
        updatedMovie.Should().NotBeNull();
        updatedMovie!.Name.Should().Be("Updated Name");
        updatedMovie.DurationMinutes.Should().Be(140);
    }

    [Fact]
    public async Task DeleteAsync_ExistingMovie_DeletesSuccessfully()
    {
        // Arrange
        var movie = new Movie
        {
            Name = "Movie to Delete",
            DurationMinutes = 90,
            AgeLimit = 7,
            Genre = MovieGenre.Animation,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();
        var movieId = movie.Id;

        // Act
        await _repository.DeleteAsync(movie);

        // Assert
        var deletedMovie = await _context.Movies.FindAsync(movieId);
        deletedMovie.Should().BeNull();
    }

    [Fact]
    public async Task GetByGenreAsync_ReturnsOnlyMatchingGenre()
    {
        // Arrange
        var movies = new List<Movie>
        {
            new Movie 
            { 
                Name = "Action 1", 
                DurationMinutes = 120, 
                AgeLimit = 13, 
                Genre = MovieGenre.Action,
                ReleaseDate = new DateOnly(2024, 1, 1)
            },
            new Movie 
            { 
                Name = "Drama 1", 
                DurationMinutes = 130, 
                AgeLimit = 16, 
                Genre = MovieGenre.Drama,
                ReleaseDate = new DateOnly(2024, 1, 1)
            },
            new Movie 
            { 
                Name = "Action 2", 
                DurationMinutes = 110, 
                AgeLimit = 13, 
                Genre = MovieGenre.Action,
                ReleaseDate = new DateOnly(2024, 1, 1)
            }
        };

        await _context.Movies.AddRangeAsync(movies);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByGenreAsync(MovieGenre.Action);
        var resultList = result.ToList();

        // Assert
        resultList.Should().HaveCount(2);
        resultList.Should().OnlyContain(m => m.Genre == MovieGenre.Action);
        resultList.Should().Contain(m => m.Name == "Action 1");
        resultList.Should().Contain(m => m.Name == "Action 2");
    }

    [Fact]
    public async Task SearchByNameAsync_ValidTerm_ReturnsMatchingMovies()
    {
        // Arrange
        var movies = new List<Movie>
        {
            new Movie 
            { 
                Name = "The Matrix", 
                DurationMinutes = 136, 
                AgeLimit = 13, 
                Genre = MovieGenre.SciFi,
                ReleaseDate = new DateOnly(1999, 3, 31)
            },
            new Movie 
            { 
                Name = "Matrix Reloaded", 
                DurationMinutes = 138, 
                AgeLimit = 13, 
                Genre = MovieGenre.SciFi,
                ReleaseDate = new DateOnly(2003, 5, 15)
            },
            new Movie 
            { 
                Name = "Inception", 
                DurationMinutes = 148, 
                AgeLimit = 13, 
                Genre = MovieGenre.SciFi,
                ReleaseDate = new DateOnly(2010, 7, 16)
            }
        };

        await _context.Movies.AddRangeAsync(movies);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchByNameAsync("Matrix");
        var resultList = result.ToList();

        // Assert
        resultList.Should().HaveCount(2);
        resultList.Should().OnlyContain(m => m.Name.Contains("Matrix"));
    }

    [Fact]
    public async Task SearchByNameAsync_EmptyTerm_ReturnsEmptyList()
    {
        // Arrange
        var movie = new Movie
        {
            Name = "Test Movie",
            DurationMinutes = 120,
            AgeLimit = 13,
            Genre = MovieGenre.Drama,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchByNameAsync("");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExistsAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var movie = new Movie
        {
            Name = "Existing Movie",
            DurationMinutes = 120,
            AgeLimit = 13,
            Genre = MovieGenre.Comedy,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(movie.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistingId_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_WithActorsAndDirectors_SavesRelationships()
    {
        // Arrange
        var actor1 = new Person { Name = "Actor 1" };
        var actor2 = new Person { Name = "Actor 2" };
        var director = new Person { Name = "Director 1" };

        await _context.People.AddRangeAsync(actor1, actor2, director);
        await _context.SaveChangesAsync();

        var movie = new Movie
        {
            Name = "Movie with Cast",
            DurationMinutes = 120,
            AgeLimit = 13,
            Genre = MovieGenre.Drama,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        movie.Actors.Add(actor1);
        movie.Actors.Add(actor2);
        movie.Directors.Add(director);

        // Act
        var result = await _repository.CreateAsync(movie);

        // Assert
        var savedMovie = await _context.Movies
            .Include(m => m.Actors)
            .Include(m => m.Directors)
            .FirstOrDefaultAsync(m => m.Id == result.Id);

        savedMovie.Should().NotBeNull();
        savedMovie!.Actors.Should().HaveCount(2);
        savedMovie.Directors.Should().HaveCount(1);
    }
}