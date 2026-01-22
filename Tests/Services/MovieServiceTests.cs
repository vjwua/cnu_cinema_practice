namespace Tests.Services;

using AutoMapper;
using Core.DTOs.Movies;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Services;
using FluentAssertions;
using Moq;
using Xunit;


public class MovieServiceTests
{
    private readonly Mock<IMovieRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly MovieService _service;

    public MovieServiceTests()
    {
        _mockRepository = new Mock<IMovieRepository>();
        _mockMapper = new Mock<IMapper>();
        _service = new MovieService(_mockRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsListOfMovies()
    {
        // Arrange
        var movies = new List<Movie>
        {
            new Movie { Id = 1, Name = "Movie 1", Genre = MovieGenre.Action },
            new Movie { Id = 2, Name = "Movie 2", Genre = MovieGenre.Drama }
        };

        var movieDtos = new List<MovieListDTO>
        {
            new MovieListDTO { Id = 1, Name = "Movie 1", Genre = MovieGenre.Action },
            new MovieListDTO { Id = 2, Name = "Movie 2", Genre = MovieGenre.Drama }
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(movies);

        _mockMapper.Setup(m => m.Map<IEnumerable<MovieListDTO>>(movies))
            .Returns(movieDtos);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(movieDtos);
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsMovie()
    {
        // Arrange
        var movie = new Movie 
        { 
            Id = 1, 
            Name = "Test Movie",
            Genre = MovieGenre.Action,
            DurationMinutes = 120
        };

        var movieDto = new MovieDetailDTO 
        { 
            Id = 1, 
            Name = "Test Movie",
            Genre = MovieGenre.Action,
            DurationMinutes = 120
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(movie);

        _mockMapper.Setup(m => m.Map<MovieDetailDTO>(movie))
            .Returns(movieDto);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test Movie");
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Movie?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(999), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsCreatedMovie()
    {
        // Arrange
        var createDto = new CreateMovieDTO
        {
            Name = "New Movie",
            DurationMinutes = 150,
            AgeLimit = 13,
            Genre = MovieGenre.Action,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        var movie = new Movie
        {
            Id = 1,
            Name = "New Movie",
            DurationMinutes = 150,
            AgeLimit = 13,
            Genre = MovieGenre.Action,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        var movieDetailDto = new MovieDetailDTO
        {
            Id = 1,
            Name = "New Movie",
            DurationMinutes = 150,
            AgeLimit = 13,
            Genre = MovieGenre.Action,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        _mockMapper.Setup(m => m.Map<Movie>(createDto))
            .Returns(movie);

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Movie>()))
            .ReturnsAsync(movie);

        _mockMapper.Setup(m => m.Map<MovieDetailDTO>(movie))
            .Returns(movieDetailDto);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Movie");
        result.Id.Should().Be(1);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Movie>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingMovie_UpdatesSuccessfully()
    {
        // Arrange
        var updateDto = new UpdateMovieDTO
        {
            Id = 1,
            Name = "Updated Movie",
            DurationMinutes = 140,
            AgeLimit = 16,
            Genre = MovieGenre.Drama,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        var existingMovie = new Movie
        {
            Id = 1,
            Name = "Original Movie",
            DurationMinutes = 120,
            AgeLimit = 13,
            Genre = MovieGenre.Action,
            ReleaseDate = new DateOnly(2024, 1, 1)
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingMovie);

        _mockMapper.Setup(m => m.Map(updateDto, existingMovie))
            .Returns(existingMovie);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Movie>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(updateDto);

        // Assert
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Movie>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingMovie_ThrowsKeyNotFoundException()
    {
        // Arrange
        var updateDto = new UpdateMovieDTO { Id = 999 };

        _mockRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Movie?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _service.UpdateAsync(updateDto));

        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Movie>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ExistingMovie_DeletesSuccessfully()
    {
        // Arrange
        var movie = new Movie { Id = 1, Name = "Movie to Delete" };

        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(movie);

        _mockRepository.Setup(r => r.DeleteAsync(movie))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(1);

        // Assert
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(movie), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingMovie_ThrowsKeyNotFoundException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Movie?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _service.DeleteAsync(999));

        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Movie>()), Times.Never);
    }

    [Fact]
    public async Task GetByGenreAsync_ReturnsFilteredMovies()
    {
        // Arrange
        var genre = MovieGenre.Action;
        var movies = new List<Movie>
        {
            new Movie { Id = 1, Name = "Action Movie 1", Genre = MovieGenre.Action },
            new Movie { Id = 2, Name = "Action Movie 2", Genre = MovieGenre.Action }
        };

        var movieDtos = new List<MovieListDTO>
        {
            new MovieListDTO { Id = 1, Name = "Action Movie 1", Genre = MovieGenre.Action },
            new MovieListDTO { Id = 2, Name = "Action Movie 2", Genre = MovieGenre.Action }
        };

        _mockRepository.Setup(r => r.GetByGenreAsync(genre))
            .ReturnsAsync(movies);

        _mockMapper.Setup(m => m.Map<IEnumerable<MovieListDTO>>(movies))
            .Returns(movieDtos);

        // Act
        var result = await _service.GetByGenreAsync(genre);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(m => m.Genre == MovieGenre.Action);
    }

    [Fact]
    public async Task SearchByNameAsync_ValidSearchTerm_ReturnsMatchingMovies()
    {
        // Arrange
        var searchTerm = "Matrix";
        var movies = new List<Movie>
        {
            new Movie { Id = 1, Name = "The Matrix" },
            new Movie { Id = 2, Name = "Matrix Reloaded" }
        };

        var movieDtos = new List<MovieListDTO>
        {
            new MovieListDTO { Id = 1, Name = "The Matrix" },
            new MovieListDTO { Id = 2, Name = "Matrix Reloaded" }
        };

        _mockRepository.Setup(r => r.SearchByNameAsync(searchTerm))
            .ReturnsAsync(movies);

        _mockMapper.Setup(m => m.Map<IEnumerable<MovieListDTO>>(movies))
            .Returns(movieDtos);

        // Act
        var result = await _service.SearchByNameAsync(searchTerm);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(m => m.Name.Contains("Matrix"));
    }
}