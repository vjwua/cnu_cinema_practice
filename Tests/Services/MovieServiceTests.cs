using AutoMapper;
using Core.DTOs.Movies;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Services;
using FluentAssertions;
using Moq;

namespace Tests.Services;

public class MovieServiceTests
{
    private readonly Mock<IMovieRepository> _movieRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly MovieService _service;

    public MovieServiceTests()
    {
        _movieRepoMock = new Mock<IMovieRepository>();
        _mapperMock = new Mock<IMapper>();
        
        _service = new MovieService(_movieRepoMock.Object, _mapperMock.Object);
    }

    private void SetId(Movie entity, int id)
    {
        var propInfo = entity.GetType().GetProperty("Id");
        if (propInfo != null) propInfo.SetValue(entity, id);
    }

    private Movie CreateMovieEntity(string name)
    {
        return new Movie
        {
            Name = name,
            ReleaseDate = new DateOnly(2023, 1, 1),
            DurationMinutes = 120,
            Description = "Test Desc",
            PosterUrl = "test.jpg",
            Genre = MovieGenre.Action,
            ImdbRating = 8.5m
        };
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedDtos_WhenMoviesExist()
    {
        var movies = new List<Movie> 
        { 
            CreateMovieEntity("Inception"), 
            CreateMovieEntity("Matrix") 
        };

        var dtos = new List<MovieListDTO>
        {
            new MovieListDTO { Name = "Inception" },
            new MovieListDTO { Name = "Matrix" }
        };

        _movieRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(movies);
            
        _mapperMock.Setup(m => m.Map<IEnumerable<MovieListDTO>>(movies))
            .Returns(dtos);

        var result = await _service.GetAllAsync();

        result.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDto_WhenMovieExists()
    {
        var movie = CreateMovieEntity("Dune");
        SetId(movie, 1);
        
        var dto = new MovieDetailDTO { Name = "Dune" };

        _movieRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(movie);
            
        _mapperMock.Setup(m => m.Map<MovieDetailDTO>(movie))
            .Returns(dto);

        var result = await _service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result.Name.Should().Be("Dune");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenMovieDoesNotExist()
    {
        _movieRepoMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Movie?)null);

        var result = await _service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByGenreAsync_ShouldReturnMappedDtos_WhenMoviesExist()
    {
        var genre = MovieGenre.Action;
        var movies = new List<Movie> { CreateMovieEntity("Action Movie") };
        var dtos = new List<MovieListDTO> { new MovieListDTO { Name = "Action Movie" } };

        _movieRepoMock.Setup(r => r.GetByGenreAsync(genre))
            .ReturnsAsync(movies);
            
        _mapperMock.Setup(m => m.Map<IEnumerable<MovieListDTO>>(movies))
            .Returns(dtos);

        var result = await _service.GetByGenreAsync(genre);

        result.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateMovie_AndReturnDto()
    {
        var createDto = new CreateMovieDTO 
        { 
            Name = "New Movie",
            ReleaseDate = new DateOnly(2023, 1, 1),
            DurationMinutes = 100
        };
        
        var movieEntity = CreateMovieEntity("New Movie");
        var resultDto = new MovieDetailDTO { Name = "New Movie" };

        _mapperMock.Setup(m => m.Map<Movie>(createDto)).Returns(movieEntity);
        
        _movieRepoMock.Setup(r => r.CreateAsync(movieEntity))
            .ReturnsAsync(movieEntity);
            
        _mapperMock.Setup(m => m.Map<MovieDetailDTO>(movieEntity))
            .Returns(resultDto);

        var result = await _service.CreateAsync(createDto);

        result.Should().Be(resultDto);
        _movieRepoMock.Verify(r => r.CreateAsync(movieEntity), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateMovie_WhenMovieExists()
    {
        var updateDto = new UpdateMovieDTO 
        { 
            Id = 1, 
            Name = "Updated Name" 
        };
        
        var existingMovie = CreateMovieEntity("Old Name");
        SetId(existingMovie, 1);

        _movieRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingMovie);

        await _service.UpdateAsync(updateDto);

        _mapperMock.Verify(m => m.Map(updateDto, existingMovie), Times.Once);
        _movieRepoMock.Verify(r => r.UpdateAsync(existingMovie), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenMovieDoesNotExist()
    {
        var updateDto = new UpdateMovieDTO { Id = 999 };
        _movieRepoMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Movie?)null);

        Func<Task> action = async () => await _service.UpdateAsync(updateDto);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Movie with id {updateDto.Id} not found.");
        
        _movieRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Movie>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteMovie_WhenMovieExists()
    {
        var movie = CreateMovieEntity("To Delete");
        SetId(movie, 10);

        _movieRepoMock.Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(movie);

        await _service.DeleteAsync(10);

        _movieRepoMock.Verify(r => r.DeleteAsync(movie), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowKeyNotFoundException_WhenMovieDoesNotExist()
    {
        _movieRepoMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Movie?)null);

        Func<Task> action = async () => await _service.DeleteAsync(999);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Movie with id 999 not found.");

        _movieRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Movie>()), Times.Never);
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldReturnMappedDtos_WhenMatchesFound()
    {
        var searchTerm = "Avatar";
        var movies = new List<Movie> { CreateMovieEntity("Avatar") };
        var dtos = new List<MovieListDTO> { new MovieListDTO { Name = "Avatar" } };

        _movieRepoMock.Setup(r => r.SearchByNameAsync(searchTerm))
            .ReturnsAsync(movies);
            
        _mapperMock.Setup(m => m.Map<IEnumerable<MovieListDTO>>(movies))
            .Returns(dtos);

        var result = await _service.SearchByNameAsync(searchTerm);

        result.Should().BeEquivalentTo(dtos);
        _movieRepoMock.Verify(r => r.SearchByNameAsync(searchTerm), Times.Once);
    }
}