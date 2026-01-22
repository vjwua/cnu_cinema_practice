using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Core.Entities;
using Core.Enums;
using FluentAssertions;

namespace Tests.Repositories;

public class MovieRepositoryTests
{
    private CinemaDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CinemaDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CinemaDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddMovie_WhenValid()
    {
        var dbName = Guid.NewGuid().ToString();
        var movie = new Movie 
        { 
            Name = "Dune", 
            ReleaseDate = DateOnly.FromDateTime(DateTime.Now),
            Genre = MovieGenre.SciFi 
        };

        await using (var context = GetDbContext(dbName))
        {
            var repo = new MovieRepository(context);
            await repo.CreateAsync(movie);
        }

        await using (var context = GetDbContext(dbName))
        {
            var savedMovie = await context.Movies.FirstOrDefaultAsync();
            
            savedMovie.Should().NotBeNull();
            savedMovie.Name.Should().Be("Dune");
        }
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMovie_WhenExists()
    {
        var dbName = Guid.NewGuid().ToString();
        int movieId;
        
        await using (var context = GetDbContext(dbName))
        {
            var movie = new Movie 
            { 
                Name = "Inception", 
                ReleaseDate = DateOnly.FromDateTime(DateTime.Now),
                Genre = MovieGenre.SciFi 
            };
            context.Movies.Add(movie);
            await context.SaveChangesAsync();
            movieId = movie.Id;
        }

        await using (var context = GetDbContext(dbName))
        {
            var repo = new MovieRepository(context);
            var result = await repo.GetByIdAsync(movieId);

            result.Should().NotBeNull();
            result.Name.Should().Be("Inception");
        }
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenMovieDoesNotExist()
    {
        var dbName = Guid.NewGuid().ToString();
        
        await using (var context = GetDbContext(dbName))
        {
            var repo = new MovieRepository(context);
            var result = await repo.GetByIdAsync(999);

            result.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllMovies_OrderedByReleaseDate()
    {
        var dbName = Guid.NewGuid().ToString();
        await using (var context = GetDbContext(dbName))
        {
            context.Movies.AddRange(
                new Movie { Name = "Old Movie", ReleaseDate = new DateOnly(2000, 1, 1) },
                new Movie { Name = "New Movie", ReleaseDate = new DateOnly(2025, 1, 1) }
            );
            await context.SaveChangesAsync();
        }

        await using (var context = GetDbContext(dbName))
        {
            var repo = new MovieRepository(context);
            var result = (await repo.GetAllAsync()).ToList();

            result.Should().HaveCount(2);
            result.First().Name.Should().Be("New Movie");
            result.Last().Name.Should().Be("Old Movie");
        }
    }

    [Fact]
    public async Task GetByGenreAsync_ShouldReturnOnlyMatchingMovies()
    {
        var dbName = Guid.NewGuid().ToString();
        await using (var context = GetDbContext(dbName))
        {
            context.Movies.AddRange(
                new Movie { Name = "Horror 1", Genre = MovieGenre.Horror, ReleaseDate = DateOnly.FromDateTime(DateTime.Now) },
                new Movie { Name = "Comedy 1", Genre = MovieGenre.Comedy, ReleaseDate = DateOnly.FromDateTime(DateTime.Now) },
                new Movie { Name = "Horror 2", Genre = MovieGenre.Horror, ReleaseDate = DateOnly.FromDateTime(DateTime.Now) }
            );
            await context.SaveChangesAsync();
        }

        await using (var context = GetDbContext(dbName))
        {
            var repo = new MovieRepository(context);
            var result = (await repo.GetByGenreAsync(MovieGenre.Horror)).ToList();

            result.Should().HaveCount(2);
            result.All(m => m.Genre == MovieGenre.Horror).Should().BeTrue();
        }
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldReturnMovies_WhenNameContainsQuery()
    {
        var dbName = Guid.NewGuid().ToString();
        await using (var context = GetDbContext(dbName))
        {
            context.Movies.AddRange(
                new Movie { Name = "Harry Potter 1", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) },
                new Movie { Name = "Spider Man", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) },
                new Movie { Name = "Harry Potter 2", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) }
            );
            await context.SaveChangesAsync();
        }

        await using (var context = GetDbContext(dbName))
        {
            var repo = new MovieRepository(context);
            var result = (await repo.SearchByNameAsync("Potter")).ToList();

            result.Should().HaveCount(2);
            result.All(m => m.Name.Contains("Potter")).Should().BeTrue();
        }
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldReturnEmpty_WhenQueryIsWhiteSpace()
    {
        var dbName = Guid.NewGuid().ToString();
        await using (var context = GetDbContext(dbName))
        {
            context.Movies.Add(new Movie { Name = "Test", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) });
            await context.SaveChangesAsync();
        }

        await using (var context = GetDbContext(dbName))
        {
            var repo = new MovieRepository(context);
            var result = await repo.SearchByNameAsync("   ");

            result.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyMovie()
    {
        var dbName = Guid.NewGuid().ToString();
        int movieId;
        
        await using (var context = GetDbContext(dbName))
        {
            var movie = new Movie { Name = "Old Name", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
            context.Movies.Add(movie);
            await context.SaveChangesAsync();
            movieId = movie.Id;
        }

        await using (var context = GetDbContext(dbName))
        {
            var repo = new MovieRepository(context);
            var movieToUpdate = await repo.GetByIdAsync(movieId);
            
            movieToUpdate!.Name = "New Name";
            
            await repo.UpdateAsync(movieToUpdate);
        }

        await using (var context = GetDbContext(dbName))
        {
            var updatedMovie = await context.Movies.FindAsync(movieId);
            updatedMovie.Should().NotBeNull();
            updatedMovie.Name.Should().Be("New Name");
        }
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveMovie()
    {
        var dbName = Guid.NewGuid().ToString();
        int movieId;
        
        await using (var context = GetDbContext(dbName))
        {
            var movie = new Movie { Name = "To Delete", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
            context.Movies.Add(movie);
            await context.SaveChangesAsync();
            movieId = movie.Id;
        }

        await using (var context = GetDbContext(dbName))
        {
            var repo = new MovieRepository(context);
            var movieToDelete = await repo.GetByIdAsync(movieId);
            
            await repo.DeleteAsync(movieToDelete!);
        }

        await using (var context = GetDbContext(dbName))
        {
            var deletedMovie = await context.Movies.FindAsync(movieId);
            deletedMovie.Should().BeNull();
        }
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenExists()
    {
        var dbName = Guid.NewGuid().ToString();
        int movieId;
        await using (var context = GetDbContext(dbName))
        {
            var movie = new Movie { Name = "Exist", ReleaseDate = DateOnly.FromDateTime(DateTime.Now) };
            context.Movies.Add(movie);
            await context.SaveChangesAsync();
            movieId = movie.Id;
        }

        await using (var context = GetDbContext(dbName))
        {
            var repo = new MovieRepository(context);
            var exists = await repo.ExistsAsync(movieId);
            var notExists = await repo.ExistsAsync(999);

            exists.Should().BeTrue();
            notExists.Should().BeFalse();
        }
    }
}