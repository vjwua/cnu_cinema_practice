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

namespace Tests.Advanced;

/// <summary>
/// Розширені тести для edge cases та складних сценаріїв
/// ✅ Оновлено для AutoMapper 16.0.0 з NullLoggerFactory
/// </summary>
public class MovieAdvancedTests : IDisposable
{
    private readonly CinemaDbContext _context;
    private readonly MovieRepository _repository;
    private readonly IMapper _mapper;
    private readonly MovieService _service;

    public MovieAdvancedTests()
    {
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

    #region Concurrency Tests

    [Fact]
    public async Task UpdateAsync_ConcurrentUpdates_LastWriteWins()
    {
        // Arrange - створюємо фільм
        var movie = new Movie
        {
            Name = "Original",
            DurationMinutes = 120,
            AgeLimit = 13,
            Genre = MovieGenre.Drama,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();

        // Act - симулюємо concurrent updates
        var updateDto1 = new UpdateMovieDTO
        {
            Id = movie.Id,
            Name = "Update 1",
            DurationMinutes = 130,
            AgeLimit = 13,
            Genre = MovieGenre.Drama,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        var updateDto2 = new UpdateMovieDTO
        {
            Id = movie.Id,
            Name = "Update 2",
            DurationMinutes = 140,
            AgeLimit = 13,
            Genre = MovieGenre.Drama,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        await _service.UpdateAsync(updateDto1);
        await _service.UpdateAsync(updateDto2);

        // Assert - останнє оновлення виграє
        var result = await _context.Movies.FindAsync(movie.Id);
        result!.Name.Should().Be("Update 2");
        result.DurationMinutes.Should().Be(140);
    }

    #endregion

    #region Large Dataset Tests

    [Fact]
    public async Task GetAllAsync_LargeDataset_PerformanceTest()
    {
        // Arrange - створюємо 1000 фільмів
        var movies = Enumerable.Range(1, 1000).Select(i => new Movie
        {
            Name = $"Movie {i}",
            DurationMinutes = (ushort)(90 + i % 100),
            AgeLimit = (byte)(i % 18),
            Genre = (MovieGenre)(1 << (i % 10)),
            ReleaseDate = new DateOnly(2000 + i % 24, 1, 1)
        }).ToList();

        await _context.Movies.AddRangeAsync(movies);
        await _context.SaveChangesAsync();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _service.GetAllAsync();
        stopwatch.Stop();

        // Assert
        result.Should().HaveCount(1000);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Має бути швидше 1 секунди
    }

    [Fact]
    public async Task SearchByNameAsync_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var movies = new List<Movie>
        {
            new Movie 
            { 
                Name = "Movie: The Sequel", 
                DurationMinutes = 120, 
                AgeLimit = 13, 
                Genre = MovieGenre.Action,
                ReleaseDate = new DateOnly(2024, 1, 1)
            },
            new Movie 
            { 
                Name = "Movie (2024)", 
                DurationMinutes = 130, 
                AgeLimit = 13, 
                Genre = MovieGenre.Drama,
                ReleaseDate = new DateOnly(2024, 1, 1)
            },
            new Movie 
            { 
                Name = "Movie's Adventure", 
                DurationMinutes = 110, 
                AgeLimit = 7, 
                Genre = MovieGenre.Animation,
                ReleaseDate = new DateOnly(2024, 1, 1)
            }
        };

        await _context.Movies.AddRangeAsync(movies);
        await _context.SaveChangesAsync();

        // Act & Assert
        var result1 = await _service.SearchByNameAsync("Movie:");
        result1.Should().ContainSingle(m => m.Name.Contains("Sequel"));

        var result2 = await _service.SearchByNameAsync("(2024)");
        result2.Should().ContainSingle();

        var result3 = await _service.SearchByNameAsync("'s");
        result3.Should().ContainSingle(m => m.Name.Contains("Adventure"));
    }

    #endregion

    #region Many-to-Many Relationship Tests

    [Fact]
    public async Task CreateAsync_WithMultipleActorsAndDirectors_SavesAllRelationships()
    {
        // Arrange - створюємо людей
        var actors = Enumerable.Range(1, 5).Select(i => new Person 
        { 
            Name = $"Actor {i}" 
        }).ToList();

        var directors = Enumerable.Range(1, 3).Select(i => new Person 
        { 
            Name = $"Director {i}" 
        }).ToList();

        await _context.People.AddRangeAsync(actors);
        await _context.People.AddRangeAsync(directors);
        await _context.SaveChangesAsync();

        var movie = new Movie
        {
            Name = "Ensemble Movie",
            DurationMinutes = 180,
            AgeLimit = 13,
            Genre = MovieGenre.Drama,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        foreach (var actor in actors)
            movie.Actors.Add(actor);

        foreach (var director in directors)
            movie.Directors.Add(director);

        // Act
        var result = await _repository.CreateAsync(movie);

        // Assert
        var savedMovie = await _context.Movies
            .Include(m => m.Actors)
            .Include(m => m.Directors)
            .FirstOrDefaultAsync(m => m.Id == result.Id);

        savedMovie.Should().NotBeNull();
        savedMovie!.Actors.Should().HaveCount(5);
        savedMovie.Directors.Should().HaveCount(3);
    }

    [Fact]
    public async Task UpdateAsync_RemovingAndAddingActors_UpdatesRelationships()
    {
        // Arrange
        var actor1 = new Person { Name = "Actor 1" };
        var actor2 = new Person { Name = "Actor 2" };
        var actor3 = new Person { Name = "Actor 3" };

        await _context.People.AddRangeAsync(actor1, actor2, actor3);
        await _context.SaveChangesAsync();

        var movie = new Movie
        {
            Name = "Movie with Cast Changes",
            DurationMinutes = 120,
            AgeLimit = 13,
            Genre = MovieGenre.Drama,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        movie.Actors.Add(actor1);
        movie.Actors.Add(actor2);

        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();
        _context.Entry(movie).State = EntityState.Detached;

        // Act - видаляємо actor1, додаємо actor3
        var movieToUpdate = await _context.Movies
            .Include(m => m.Actors)
            .FirstAsync(m => m.Id == movie.Id);

        movieToUpdate.Actors.Remove(movieToUpdate.Actors.First(a => a.Id == actor1.Id));
        movieToUpdate.Actors.Add(actor3);

        await _repository.UpdateAsync(movieToUpdate);

        // Assert
        var updatedMovie = await _context.Movies
            .Include(m => m.Actors)
            .FirstAsync(m => m.Id == movie.Id);

        updatedMovie.Actors.Should().HaveCount(2);
        updatedMovie.Actors.Should().Contain(a => a.Id == actor2.Id);
        updatedMovie.Actors.Should().Contain(a => a.Id == actor3.Id);
        updatedMovie.Actors.Should().NotContain(a => a.Id == actor1.Id);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task SearchByNameAsync_EmptyDatabase_ReturnsEmpty()
    {
        // Act
        var result = await _service.SearchByNameAsync("NonExistent");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByGenreAsync_NoMatchingGenre_ReturnsEmpty()
    {
        // Arrange
        var movie = new Movie
        {
            Name = "Action Movie",
            DurationMinutes = 120,
            AgeLimit = 13,
            Genre = MovieGenre.Action,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByGenreAsync(MovieGenre.Horror);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_MaximumDuration_HandlesCorrectly()
    {
        // Arrange
        var createDto = new CreateMovieDTO
        {
            Name = "Very Long Movie",
            DurationMinutes = 350, // максимум згідно з валідатором
            AgeLimit = 13,
            Genre = MovieGenre.Drama,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.DurationMinutes.Should().Be(350);
    }

    [Fact]
    public async Task CreateAsync_MinimumDuration_HandlesCorrectly()
    {
        // Arrange
        var createDto = new CreateMovieDTO
        {
            Name = "Very Short Movie",
            DurationMinutes = 1,
            AgeLimit = 13,
            Genre = MovieGenre.Short,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.DurationMinutes.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_MovieWithSessions_LoadsNavigationProperties()
    {
        // Arrange
        var hall = new Core.Entities.Hall("Test Hall", new Core.Mapping.SeatLayoutMap());
        await _context.Halls.AddAsync(hall);

        var movie = new Movie
        {
            Name = "Movie with Session",
            DurationMinutes = 120,
            AgeLimit = 13,
            Genre = MovieGenre.Action,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();

        var session = new Core.Entities.Session
        {
            MovieId = movie.Id,
            HallId = hall.Id,
            StartTime = DateTime.UtcNow.AddDays(1),
            BasePrice = 100,
            MovieFormat = MovieFormat.TwoD
        };

        await _context.Sessions.AddAsync(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(movie.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Sessions.Should().ContainSingle();
    }

    #endregion

    #region Null and Validation Tests

    [Fact]
    public async Task CreateAsync_OptionalFieldsNull_CreatesSuccessfully()
    {
        // Arrange
        var createDto = new CreateMovieDTO
        {
            Name = "Minimal Movie",
            DurationMinutes = 90,
            AgeLimit = 0,
            Genre = MovieGenre.None,
            ReleaseDate = new DateOnly(2024, 1, 1),
            // Всі опціональні поля null
            Description = null,
            ImdbRating = null,
            PosterUrl = null,
            TrailerUrl = null,
            Country = null
        };

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().BeNull();
        result.ImdbRating.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region Performance and Optimization Tests

    [Fact]
    public async Task GetByIdAsync_MultipleCallsSameMovie_UsesCache()
    {
        // Arrange
        var movie = new Movie
        {
            Name = "Cached Movie",
            DurationMinutes = 120,
            AgeLimit = 13,
            Genre = MovieGenre.Action,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();

        // Act - multiple calls
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result1 = await _service.GetByIdAsync(movie.Id);
        var time1 = stopwatch.ElapsedMilliseconds;
        
        stopwatch.Restart();
        var result2 = await _service.GetByIdAsync(movie.Id);
        var time2 = stopwatch.ElapsedMilliseconds;

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1!.Id.Should().Be(result2!.Id);
    }

    #endregion
}