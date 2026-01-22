using AutoMapper;
using Tests.Helpers;
using Core.DTOs.Movies;
using Core.Entities;
using Core.Enums;
using Core.Mapping;
using Core.Services;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Tests.Integration;

/// <summary>
/// Integration тести для MovieService
/// ✅ Оновлено для AutoMapper 16.0.0 з NullLoggerFactory
/// </summary>
public class MovieServiceIntegrationTests : IDisposable
{
    private readonly CinemaDbContext _context;
    private readonly MovieRepository _repository;
    private readonly IMapper _mapper;
    private readonly MovieService _service;

    public MovieServiceIntegrationTests()
    {
        // Налаштування InMemory бази
        var options = new DbContextOptionsBuilder<CinemaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CinemaDbContext(options);
        _repository = new MovieRepository(_context);

        // ✅ AutoMapper 16.0.0 - використовуємо NullLoggerFactory.Instance
        var configExpression = new MapperConfigurationExpression();
        configExpression.AddProfile<MovieMapping>();
        
        // ✅ NullLoggerFactory.Instance замість null
        var config = new MapperConfiguration(configExpression, NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();

        _service = new MovieService(_repository, _mapper);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateAsync_CompleteFlow_CreatesMovieSuccessfully()
    {
        // Arrange
        var createDto = new CreateMovieDTO
        {
            Name = "Test Movie",
            DurationMinutes = 120,
            AgeLimit = 13,
            Genre = MovieGenre.Action,
            Description = "A test movie",
            ReleaseDate = new DateOnly(2024, 1, 1),
            ImdbRating = 8.5m,
            Country = "USA"
        };

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Test Movie");
        result.DurationMinutes.Should().Be(120);

        // Перевірка в базі
        var movieInDb = await _context.Movies.FindAsync(result.Id);
        movieInDb.Should().NotBeNull();
        movieInDb!.Name.Should().Be("Test Movie");
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleMovies_ReturnsAllOrderedByDate()
    {
        // Arrange
        var movies = new List<Movie>
        {
            new Movie 
            { 
                Name = "Old Movie", 
                DurationMinutes = 120, 
                AgeLimit = 13, 
                Genre = MovieGenre.Drama,
                ReleaseDate = new DateOnly(2020, 1, 1)
            },
            new Movie 
            { 
                Name = "New Movie", 
                DurationMinutes = 130, 
                AgeLimit = 16, 
                Genre = MovieGenre.Action,
                ReleaseDate = new DateOnly(2024, 1, 1)
            }
        };

        await _context.Movies.AddRangeAsync(movies);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllAsync();
        var resultList = result.ToList();

        // Assert
        resultList.Should().HaveCount(2);
        resultList[0].Name.Should().Be("New Movie");
        resultList[0].ReleaseDate.Should().Be(new DateOnly(2024, 1, 1));
    }

    [Fact]
    public async Task UpdateAsync_CompleteFlow_UpdatesMovieSuccessfully()
    {
        // Arrange - створюємо фільм
        var movie = new Movie
        {
            Name = "Original Name",
            DurationMinutes = 120,
            AgeLimit = 13,
            Genre = MovieGenre.Drama,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();

        // Act - оновлюємо
        var updateDto = new UpdateMovieDTO
        {
            Id = movie.Id,
            Name = "Updated Name",
            DurationMinutes = 140,
            AgeLimit = 16,
            Genre = MovieGenre.Action,
            ReleaseDate = new DateOnly(2024, 6, 1),
            Description = "Updated description"
        };

        await _service.UpdateAsync(updateDto);

        // Assert
        var updatedMovie = await _context.Movies.FindAsync(movie.Id);
        updatedMovie.Should().NotBeNull();
        updatedMovie!.Name.Should().Be("Updated Name");
        updatedMovie.DurationMinutes.Should().Be(140);
        updatedMovie.AgeLimit.Should().Be(16);
        updatedMovie.Genre.Should().Be(MovieGenre.Action);
    }

    [Fact]
    public async Task GetByIdAsync_WithActorsAndDirectors_ReturnsCompleteDetails()
    {
        // Arrange
        var actor1 = new Person { Name = "Tom Hanks" };
        var actor2 = new Person { Name = "Morgan Freeman" };
        var director = new Person { Name = "Steven Spielberg" };

        var movie = new Movie
        {
            Name = "Great Movie",
            DurationMinutes = 180,
            AgeLimit = 13,
            Genre = MovieGenre.Drama,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        movie.Actors.Add(actor1);
        movie.Actors.Add(actor2);
        movie.Directors.Add(director);

        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(movie.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Great Movie");
        result.Actors.Should().HaveCount(2);
        result.Directors.Should().HaveCount(1);
        result.Actors.Should().Contain(a => a.Name == "Tom Hanks");
        result.Directors.First().Name.Should().Be("Steven Spielberg");
    }

    [Fact]
    public async Task SearchByNameAsync_CaseInsensitiveSearch_ReturnsMatches()
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
                ReleaseDate = new DateOnly(1999, 1, 1)
            },
            new Movie 
            { 
                Name = "Matrix Reloaded", 
                DurationMinutes = 138, 
                AgeLimit = 13, 
                Genre = MovieGenre.SciFi,
                ReleaseDate = new DateOnly(2003, 1, 1)
            },
            new Movie 
            { 
                Name = "Inception", 
                DurationMinutes = 148, 
                AgeLimit = 13, 
                Genre = MovieGenre.SciFi,
                ReleaseDate = new DateOnly(2010, 1, 1)
            }
        };

        await _context.Movies.AddRangeAsync(movies);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.SearchByNameAsync("Matrix");
        var resultList = result.ToList();

        // Assert
        resultList.Should().HaveCount(2);
        resultList.Should().OnlyContain(m => 
            m.Name.Contains("Matrix", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task DeleteAsync_ExistingMovie_RemovesFromDatabase()
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
        await _service.DeleteAsync(movieId);

        // Assert
        var deletedMovie = await _context.Movies.FindAsync(movieId);
        deletedMovie.Should().BeNull();
    }

    [Fact]
    public async Task GetByGenreAsync_FiltersByGenreCorrectly()
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
        var result = await _service.GetByGenreAsync(MovieGenre.Action);
        var resultList = result.ToList();

        // Assert
        resultList.Should().HaveCount(2);
        resultList.Should().OnlyContain(m => m.Genre == MovieGenre.Action);
        resultList.Select(m => m.Name).Should().Contain(new[] { "Action 1", "Action 2" });
    }
}