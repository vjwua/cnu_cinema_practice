using AutoMapper;
using Core.DTOs.Seats;
using Core.Entities;
using Core.Interfaces.Repositories;
using Core.Services;
using FluentAssertions;
using Moq;

namespace Tests.Services;

public class SeatServiceTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ISeatRepository> _seatRepoMock;
    private readonly SeatService _service;

    public SeatServiceTests()
    {
        _mapperMock = new Mock<IMapper>();
        _seatRepoMock = new Mock<ISeatRepository>();

        _service = new SeatService(_mapperMock.Object, _seatRepoMock.Object);
    }

    private void SetId(Seat entity, int id)
    {
        var propInfo = entity.GetType().GetProperty("Id");
        if (propInfo != null) propInfo.SetValue(entity, id);
    }

    private Seat CreateSeatEntity(byte row, byte number)
    {
        return new Seat 
        { 
            RowNum = row,      
            SeatNum = number,  
            HallId = 1,
            SeatTypeId = 1
        }; 
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDto_WhenSeatExists()
    {
        var seatEntity = CreateSeatEntity(1, 5);
        SetId(seatEntity, 10);
        
        var expectedDto = new SeatDTO 
        { 
            Id = 10, 
            RowNum = 1, 
            SeatNum = 5,
            HallId = 1
        };

        _seatRepoMock.Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(seatEntity);
            
        _mapperMock.Setup(m => m.Map<SeatDTO>(seatEntity))
            .Returns(expectedDto);

        var result = await _service.GetByIdAsync(10);

        result.Should().NotBeNull();
        result.Id.Should().Be(10);
        result.RowNum.Should().Be(1);
        result.SeatNum.Should().Be(5);
        
        _seatRepoMock.Verify(r => r.GetByIdAsync(10), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenSeatDoesNotExist()
    {
        _seatRepoMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Seat?)null);
            
        _mapperMock.Setup(m => m.Map<SeatDTO>(null))
            .Returns((SeatDTO)null!);

        var result = await _service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySessionIdAsync_ShouldReturnMappedDtos_WhenSeatsExist()
    {
        var seats = new List<Seat> 
        { 
            CreateSeatEntity(1, 1), 
            CreateSeatEntity(1, 2) 
        };
        
        var dtos = new List<SeatDTO> 
        { 
            new SeatDTO { RowNum = 1, SeatNum = 1 }, 
            new SeatDTO { RowNum = 1, SeatNum = 2 } 
        };

        _seatRepoMock.Setup(r => r.GetBySessionIdAsync(100))
            .ReturnsAsync(seats);
            
        _mapperMock.Setup(m => m.Map<IEnumerable<SeatDTO>>(seats))
            .Returns(dtos);

        var result = (await _service.GetBySessionIdAsync(100)).ToList();

        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(dtos);
        _seatRepoMock.Verify(r => r.GetBySessionIdAsync(100), Times.Once);
    }

    [Fact]
    public async Task GetBySessionIdAsync_ShouldReturnEmptyList_WhenNoSeatsFound()
    {
        _seatRepoMock.Setup(r => r.GetBySessionIdAsync(100))
            .ReturnsAsync(new List<Seat>());
            
        _mapperMock.Setup(m => m.Map<IEnumerable<SeatDTO>>(It.IsAny<List<Seat>>()))
            .Returns(new List<SeatDTO>());

        var result = (await _service.GetBySessionIdAsync(100)).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAvailableSeatsAsync_ShouldReturnMappedDtos_ForAvailableSeats()
    {
        var availableSeats = new List<Seat> { CreateSeatEntity(2, 5) };
        var dtos = new List<SeatDTO> { new SeatDTO { RowNum = 2, SeatNum = 5 } };

        _seatRepoMock.Setup(r => r.GetAvailableSeatsAsync(200))
            .ReturnsAsync(availableSeats);

        _mapperMock.Setup(m => m.Map<IEnumerable<SeatDTO>>(availableSeats))
            .Returns(dtos);

        var result = (await _service.GetAvailableSeatsAsync(200)).ToList();

        result.Should().HaveCount(1);
        result.First().SeatNum.Should().Be(5);
        _seatRepoMock.Verify(r => r.GetAvailableSeatsAsync(200), Times.Once);
    }

    [Fact]
    public async Task GetAvailableSeatsAsync_ShouldReturnEmpty_WhenNoSeatsAvailable()
    {
        _seatRepoMock.Setup(r => r.GetAvailableSeatsAsync(200))
            .ReturnsAsync(new List<Seat>());

        _mapperMock.Setup(m => m.Map<IEnumerable<SeatDTO>>(It.IsAny<List<Seat>>()))
            .Returns(new List<SeatDTO>());

        var result = (await _service.GetAvailableSeatsAsync(200)).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ReserveSeatAsync_ShouldReturnTrue_WhenRepoReturnsTrue()
    {
        var seatDto = new SeatDTO { Id = 50, HallId = 1 };
        int sessionId = 1;

        _seatRepoMock.Setup(r => r.ReserveSeatAsync(seatDto.Id, sessionId, 10.0m, null))
            .ReturnsAsync(true);

        var result = await _service.ReserveSeatAsync(seatDto, sessionId, 10.0m);

        result.Should().BeTrue();
        _seatRepoMock.Verify(r => r.ReserveSeatAsync(50, sessionId, 10.0m, null), Times.Once);
    }

    [Fact]
    public async Task ReserveSeatAsync_ShouldReturnFalse_WhenRepoReturnsFalse()
    {
        var seatDto = new SeatDTO { Id = 50 };
        int sessionId = 1;

        _seatRepoMock.Setup(r => r.ReserveSeatAsync(seatDto.Id, sessionId, 10.0m, null))
            .ReturnsAsync(false);

        var result = await _service.ReserveSeatAsync(seatDto, sessionId, 10.0m);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsSeatAvailableAsync_ShouldReturnTrue_WhenRepoReturnsTrue()
    {
        var seatDto = new SeatDTO { Id = 10 };
        int sessionId = 5;

        _seatRepoMock.Setup(r => r.IsSeatAvailableAsync(seatDto.Id, sessionId))
            .ReturnsAsync(true);

        var result = await _service.IsSeatAvailableAsync(seatDto, sessionId);

        result.Should().BeTrue();
        _seatRepoMock.Verify(r => r.IsSeatAvailableAsync(10, sessionId), Times.Once);
    }

    [Fact]
    public async Task IsSeatAvailableAsync_ShouldReturnFalse_WhenRepoReturnsFalse()
    {
        var seatDto = new SeatDTO { Id = 10 };
        int sessionId = 5;

        _seatRepoMock.Setup(r => r.IsSeatAvailableAsync(seatDto.Id, sessionId))
            .ReturnsAsync(false);

        var result = await _service.IsSeatAvailableAsync(seatDto, sessionId);

        result.Should().BeFalse();
    }
}