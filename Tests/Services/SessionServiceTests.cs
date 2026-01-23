using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper;
using Core.DTOs.Sessions;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Services;
using Core.Validators.Sessions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace Tests.Services;

public class SessionServiceTests
{
    private readonly Mock<ISessionRepository> _sessionRepoMock;
    private readonly Mock<IMovieRepository> _movieRepoMock;
    private readonly Mock<IHallRepository> _hallRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateSessionDTO>> _createFluentValidatorMock;
    private readonly Mock<IValidator<UpdateSessionDTO>> _updateFluentValidatorMock;
    private readonly SessionService _service;

    public SessionServiceTests()
    {
        _sessionRepoMock = new Mock<ISessionRepository>();
        _movieRepoMock = new Mock<IMovieRepository>();
        _hallRepoMock = new Mock<IHallRepository>();
        _mapperMock = new Mock<IMapper>();
        _createFluentValidatorMock = new Mock<IValidator<CreateSessionDTO>>();
        _updateFluentValidatorMock = new Mock<IValidator<UpdateSessionDTO>>();

        var sessionBusinessValidator = new SessionBusinessValidator(
            _movieRepoMock.Object, 
            _hallRepoMock.Object, 
            _sessionRepoMock.Object);

        var createBusinessValidator = new CreateSessionBusinessValidator(sessionBusinessValidator);
        
        var updateBusinessValidator = new UpdateSessionBusinessValidator(
            sessionBusinessValidator, 
            _movieRepoMock.Object);

        _service = new SessionService(
            _sessionRepoMock.Object,
            _mapperMock.Object,
            _createFluentValidatorMock.Object,
            _updateFluentValidatorMock.Object,
            sessionBusinessValidator,
            createBusinessValidator,
            updateBusinessValidator
        );
    }

    private Hall CreateTestHall(int id, List<Seat> seats)
    {
        var hall = (Hall)RuntimeHelpers.GetUninitializedObject(typeof(Hall));

        SetProp(hall, "Id", id);
        SetProp(hall, "Seats", seats);

        return hall;
    }

    private void SetProp<T>(T entity, string propName, object value)
    {
        var prop = typeof(T).GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(entity, value);
        }
        else
        {
            var field = typeof(T).GetField($"<{propName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(entity, value);
        }
    }

    [Fact]
    public async Task GetSessionByIdAsync_ShouldReturnDto_WhenSessionExists()
    {
        var session = new Session { Id = 1 };
        var dto = new SessionDetailDTO { Id = 1 };

        _sessionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(session);
        _mapperMock.Setup(m => m.Map<SessionDetailDTO>(session)).Returns(dto);

        var result = await _service.GetSessionByIdAsync(1);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetSessionByIdAsync_ShouldReturnNull_WhenSessionDoesNotExist()
    {
        _sessionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Session?)null);
        var result = await _service.GetSessionByIdAsync(1);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllSessionsAsync_ShouldReturnMappedList()
    {
        List<Session> sessions = [new Session(), new Session()];
        _sessionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(sessions);
        _mapperMock.Setup(m => m.Map<SessionListDTO>(It.IsAny<Session>())).Returns(new SessionListDTO());

        var result = await _service.GetAllSessionsAsync();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSessionsByMovieIdAsync_ShouldReturnList()
    {
        List<Session> sessions = [new Session()];
        _sessionRepoMock.Setup(r => r.GetByMovieIdAsync(10)).ReturnsAsync(sessions);
        _mapperMock.Setup(m => m.Map<SessionListDTO>(It.IsAny<Session>())).Returns(new SessionListDTO());

        var result = await _service.GetSessionsByMovieIdAsync(10);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetSessionByIdWithSeatsAsync_ShouldMapAvailabilityCorrectly()
    {
        var now = DateTime.UtcNow;

        var seats = new List<Seat> 
        { 
            new() { Id = 1, RowNum = 1, SeatNum = 1, SeatType = new SeatType { Name = "Standard" } },
            new() { Id = 2, RowNum = 1, SeatNum = 2, SeatType = new SeatType { Name = "VIP" } },
            new() { Id = 3, RowNum = 1, SeatNum = 3, SeatType = new SeatType { Name = "Standard" } }
        };

        var hall = CreateTestHall(1, seats);

        var session = new Session
        {
            Id = 1,
            Hall = hall,
            SeatReservations =
            [
                new SeatReservation { SeatId = 1, Status = ReservationStatus.Sold },
                new SeatReservation { SeatId = 2, Status = ReservationStatus.Reserved, ExpiresAt = now.AddMinutes(10) },
                new SeatReservation { SeatId = 3, Status = ReservationStatus.Reserved, ExpiresAt = now.AddMinutes(-10) }
            ]
        };

        var dto = new SessionDetailDTO { Id = 1 };

        _sessionRepoMock.Setup(r => r.GetByIdWithSeatsAsync(1)).ReturnsAsync(session);
        _mapperMock.Setup(m => m.Map<SessionDetailDTO>(session)).Returns(dto);

        var result = await _service.GetSessionByIdWithSeatsAsync(1);

        result.Should().NotBeNull();
        result.Seats.Should().HaveCount(3);
        result.Seats.First(s => s.SeatId == 1).IsAvailable.Should().BeFalse();
        result.Seats.First(s => s.SeatId == 2).IsAvailable.Should().BeFalse();
        result.Seats.First(s => s.SeatId == 3).IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task CreateSessionAsync_ShouldSucceed_WhenValid()
    {
        var dto = new CreateSessionDTO { MovieId = 1, HallId = 1, StartTime = DateTime.UtcNow.AddDays(1) };
        var movie = new Movie { Id = 1, DurationMinutes = 120 };
        var hall = CreateTestHall(1, []);
        var createdSession = new Session { Id = 10 };
        var fullSession = new Session { Id = 10 };

        _createFluentValidatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _movieRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(movie);
        _hallRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(hall);
        _sessionRepoMock.Setup(r => r.GetByHallIdAsync(1)).ReturnsAsync([]);

        _mapperMock.Setup(m => m.Map<Session>(dto)).Returns(new Session());
        _sessionRepoMock.Setup(r => r.CreateAsync(It.IsAny<Session>())).ReturnsAsync(createdSession);
        _sessionRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(fullSession);
        _mapperMock.Setup(m => m.Map<SessionDetailDTO>(fullSession)).Returns(new SessionDetailDTO { Id = 10 });

        var result = await _service.CreateSessionAsync(dto);

        result.Id.Should().Be(10);
        _sessionRepoMock.Verify(r => r.CreateAsync(It.IsAny<Session>()), Times.Once);
    }

    [Fact]
    public async Task CreateSessionAsync_ShouldThrowValidationEx_WhenFluentValidationFails()
    {
        var dto = new CreateSessionDTO();
        var failure = new ValidationFailure("MovieId", "Error");
        
        _createFluentValidatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([failure]));

        Func<Task> act = async () => await _service.CreateSessionAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
        _sessionRepoMock.Verify(r => r.CreateAsync(It.IsAny<Session>()), Times.Never);
    }

    [Fact]
    public async Task CreateSessionAsync_ShouldThrowValidationEx_WhenScheduleConflictDetected()
    {
        var startTime = DateTime.UtcNow.AddDays(1);
        var dto = new CreateSessionDTO { MovieId = 1, HallId = 1, StartTime = startTime };
        var movie = new Movie { Id = 1, DurationMinutes = 120 };
        var hall = CreateTestHall(1, []);
        
        _createFluentValidatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        
        _movieRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(movie);
        _hallRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(hall);
        
        var conflictingSession = new Session 
        { 
            StartTime = startTime, 
            Movie = new Movie { DurationMinutes = 100 }
        };
        _sessionRepoMock.Setup(r => r.GetByHallIdAsync(1)).ReturnsAsync([conflictingSession]);

        Func<Task> act = async () => await _service.CreateSessionAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Schedule conflict detected*");
    }

    [Fact]
    public async Task UpdateSessionAsync_ShouldUpdate_WhenValid()
    {
        int sessionId = 1;
        var dto = new UpdateSessionDTO { BasePrice = 200 };
        var existingSession = new Session { Id = 1, BasePrice = 100 };

        _updateFluentValidatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _sessionRepoMock.Setup(r => r.GetByIdAsync(sessionId)).ReturnsAsync(existingSession);

        await _service.UpdateSessionAsync(sessionId, dto);

        existingSession.BasePrice.Should().Be(200);
        _sessionRepoMock.Verify(r => r.UpdateAsync(existingSession), Times.Once);
    }

    [Fact]
    public async Task UpdateSessionAsync_ShouldThrow_WhenSessionNotFound()
    {
        _updateFluentValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateSessionDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        
        _sessionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Session?)null);

        Func<Task> act = async () => await _service.UpdateSessionAsync(1, new UpdateSessionDTO());

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Session with id 1 was not found*");
    }

    [Fact]
    public async Task DeleteSessionAsync_ShouldDelete_WhenNoOrders()
    {
        _sessionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Session { Id = 1 });
        _sessionRepoMock.Setup(r => r.HasAnyOrdersAsync(1)).ReturnsAsync(false);

        await _service.DeleteSessionAsync(1);

        _sessionRepoMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }
    
    [Fact]
    public async Task GetSessionsByHallIdAsync_ShouldReturnMappedList()
    {
        _sessionRepoMock.Setup(r => r.GetByHallIdAsync(1)).ReturnsAsync([new Session()]);
        _mapperMock.Setup(m => m.Map<SessionListDTO>(It.IsAny<Session>())).Returns(new SessionListDTO());

        var result = await _service.GetSessionsByHallIdAsync(1);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetSessionsByDateRangeAsync_ShouldReturnMappedList()
    {
        var start = DateTime.Now; 
        var end = DateTime.Now.AddDays(1);
        _sessionRepoMock.Setup(r => r.GetByDateRangeAsync(start, end)).ReturnsAsync([new Session()]);
        _mapperMock.Setup(m => m.Map<SessionListDTO>(It.IsAny<Session>())).Returns(new SessionListDTO());

        var result = await _service.GetSessionsByDateRangeAsync(start, end);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetUpcomingSessionsAsync_ShouldReturnMappedList()
    {
        _sessionRepoMock.Setup(r => r.GetUpcomingSessionsAsync()).ReturnsAsync([new Session()]);
        _mapperMock.Setup(m => m.Map<SessionPreviewDTO>(It.IsAny<Session>())).Returns(new SessionPreviewDTO());

        var result = await _service.GetUpcomingSessionsAsync();
        result.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task GetSessionByIdWithSeatsAsync_ShouldReturnNull_WhenSessionNotFound()
    {
        _sessionRepoMock.Setup(r => r.GetByIdWithSeatsAsync(999)).ReturnsAsync((Session?)null);
    
        var result = await _service.GetSessionByIdWithSeatsAsync(999);
    
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task CreateSessionAsync_ShouldThrowInvalidOperation_WhenRetrievedSessionIsNull()
    {
        var dto = new CreateSessionDTO { MovieId = 1, HallId = 1, StartTime = DateTime.UtcNow.AddDays(1) };
    
        _createFluentValidatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _movieRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Movie { DurationMinutes = 120 });
        _hallRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CreateTestHall(1, []));
        _sessionRepoMock.Setup(r => r.GetByHallIdAsync(1)).ReturnsAsync([]);

        _sessionRepoMock.Setup(r => r.CreateAsync(It.IsAny<Session>())).ReturnsAsync(new Session { Id = 10 });
        _sessionRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((Session?)null);

        Func<Task> act = async () => await _service.CreateSessionAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to retrieve created session*");
    }
    
    [Fact]
    public async Task HasScheduleConflictAsync_ShouldReturnTrue_WhenConflictExists()
    {
        var startTime = DateTime.UtcNow;
        var duration = 120;

        _sessionRepoMock.Setup(r => r.GetByHallIdAsync(1))
            .ReturnsAsync([new Session { 
                StartTime = startTime, 
                Movie = new Movie { DurationMinutes = (byte)duration } 
            }]);

        var result = await _service.HasScheduleConflictAsync(1, startTime, duration);

        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task DeleteSessionAsync_ShouldThrowValidationEx_WhenSessionHasOrders()
    {
        int sessionId = 1;
        _sessionRepoMock.Setup(r => r.GetByIdAsync(sessionId)).ReturnsAsync(new Session { Id = sessionId });
    
        _sessionRepoMock.Setup(r => r.HasAnyOrdersAsync(sessionId)).ReturnsAsync(true);

        Func<Task> act = async () => await _service.DeleteSessionAsync(sessionId);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Cannot delete a session that has existing orders*");
    
        _sessionRepoMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateSessionAsync_ShouldIgnoreNullValues_InDto()
    {
        int sessionId = 1;
        var originalTime = DateTime.UtcNow;
        var existingSession = new Session 
        { 
            Id = sessionId, 
            BasePrice = 100, 
            StartTime = originalTime,
            MovieId = 5
        };

        var dto = new UpdateSessionDTO { BasePrice = 200, StartTime = null, MovieId = null };

        _updateFluentValidatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _sessionRepoMock.Setup(r => r.GetByIdAsync(sessionId)).ReturnsAsync(existingSession);

        await _service.UpdateSessionAsync(sessionId, dto);

        existingSession.BasePrice.Should().Be(200);
        existingSession.StartTime.Should().Be(originalTime);
        existingSession.MovieId.Should().Be(5);
    
        _sessionRepoMock.Verify(r => r.UpdateAsync(existingSession), Times.Once);
    }
    
    [Fact]
    public async Task UpdateSessionAsync_ShouldThrowValidationEx_WhenScheduleConflictDetected()
    {
        int sessionId = 1;
        var dto = new UpdateSessionDTO { StartTime = DateTime.UtcNow.AddHours(5) };
    
        _updateFluentValidatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    
        var existingSession = new Session { Id = sessionId, MovieId = 1, HallId = 1, Movie = new Movie { DurationMinutes = 120 } };
    
        _sessionRepoMock.Setup(r => r.GetByIdAsync(sessionId)).ReturnsAsync(existingSession);
        _movieRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Movie { DurationMinutes = 120 });
    
        _sessionRepoMock.Setup(r => r.GetByHallIdAsync(1))
            .ReturnsAsync([
                new Session { Id = 2, StartTime = dto.StartTime.Value, Movie = new Movie { DurationMinutes = 120 } }
            ]);

        Func<Task> act = async () => await _service.UpdateSessionAsync(sessionId, dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Schedule conflict detected*");
    }
}