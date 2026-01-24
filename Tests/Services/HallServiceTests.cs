using AutoMapper;
using Core.DTOs.Halls;
using Core.Entities;
using Core.Interfaces.Repositories;
using Core.Services;
using FluentAssertions;
using Moq;

namespace Tests.Services;

public class HallServiceTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IHallRepository> _hallRepoMock;
    private readonly HallService _service;

    public HallServiceTests()
    {
        _mapperMock = new Mock<IMapper>();
        _hallRepoMock = new Mock<IHallRepository>();
        
        var seatRepoMock = new Mock<ISeatRepository>();

        _service = new HallService(_mapperMock.Object, _hallRepoMock.Object, seatRepoMock.Object);
    }

    private void SetId(Hall entity, int id)
    {
        var propInfo = entity.GetType().GetProperty("Id");
        if (propInfo != null) propInfo.SetValue(entity, id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedDtos_WhenHallsExist()
    {
        var halls = new List<Hall> { new Hall("H1", 5, 5), new Hall("H2", 5, 5) };
        
        var dtos = new List<HallListDTO> 
        { 
            new HallListDTO { Name = "H1" }, 
            new HallListDTO { Name = "H2" } 
        };

        _hallRepoMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(halls);
        
        _mapperMock.Setup(m => m.Map<IEnumerable<HallListDTO>>(halls))
            .Returns(dtos);

        var result = await _service.GetAllAsync();

        result.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDto_WhenHallExists()
    {
        var hall = new Hall("Main", 10, 10);
        SetId(hall, 1); 

        var dto = new HallDetailDTO { Name = "Main" };

        _hallRepoMock.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(hall);
        
        _mapperMock.Setup(m => m.Map<HallDetailDTO>(hall))
            .Returns(dto);

        var result = await _service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result.Name.Should().Be("Main");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenRepoReturnsNull()
    {
        _hallRepoMock.Setup(repo => repo.GetByIdAsync(999))
            .ReturnsAsync((Hall?)null);
        
        _mapperMock.Setup(m => m.Map<HallDetailDTO>(null))
            .Returns((HallDetailDTO)null!); 

        var result = await _service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateHallAndSeats_AndReturnDto()
    {
        var createDto = new CreateHallDTO 
        { 
            Name = "New Hall",
            SeatLayout = new byte[,] { { 1 } }
        };
        
        var hallEntity = new Hall("New Hall", 10, 10); 
        var createdHall = new Hall("New Hall", 10, 10);
        SetId(createdHall, 10);

        var resultDto = new HallDetailDTO { Name = "New Hall" };

        _mapperMock.Setup(m => m.Map<Hall>(createDto)).Returns(hallEntity);
        _hallRepoMock.Setup(r => r.CreateAsync(hallEntity)).ReturnsAsync(createdHall);
        _mapperMock.Setup(m => m.Map<HallDetailDTO>(createdHall)).Returns(resultDto);

        var result = await _service.CreateAsync(createDto);
        result.Should().Be(resultDto);
        _hallRepoMock.Verify(r => r.CreateAsync(hallEntity), Times.Once);
        _hallRepoMock.Verify(r => r.CreateSeatsAsync(10, createDto.SeatLayout), Times.Once);
    }

    [Fact]
    public async Task UpdateHallInfo_ShouldUpdateName_WhenNameIsProvided()
    {
        var updateDto = new UpdateHallDTO 
        { 
            Id = 1, 
            Name = "Updated Name" 
        };

        await _service.UpdateHallInfo(updateDto);

        _hallRepoMock.Verify(r => r.UpdateNameAsync(1, "Updated Name"), Times.Once);
        _hallRepoMock.Verify(r => r.UpdateSeatLayoutAsync(It.IsAny<int>(), It.IsAny<byte[,]>()), Times.Never);
    }

    [Fact]
    public async Task UpdateHallInfo_ShouldUpdateLayout_WhenLayoutIsProvided()
    {
        var layout = new byte[,] { { 1 } };
        var updateDto = new UpdateHallDTO 
        { 
            Id = 1, 
            SeatLayout = layout 
        };

        await _service.UpdateHallInfo(updateDto);

        _hallRepoMock.Verify(r => r.UpdateSeatLayoutAsync(1, layout), Times.Once);
        _hallRepoMock.Verify(r => r.UpdateNameAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RemoveAllSeatsAsync_ShouldCallRepo()
    {
        await _service.RemoveAllSeatsAsync(5);
        _hallRepoMock.Verify(r => r.RemoveAllSeatsAsync(5), Times.Once);
    }

    [Fact]
    public async Task GetSeatCountAsync_ShouldReturnCountFromRepo()
    {
        _hallRepoMock.Setup(r => r.GetSeatCountAsync(5)).ReturnsAsync(42);
        var result = await _service.GetSeatCountAsync(5);
        result.Should().Be(42);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnResultFromRepo()
    {
        _hallRepoMock.Setup(r => r.ExistsAsync(5)).ReturnsAsync(true);
        var result = await _service.ExistsAsync(5);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepo()
    {
        await _service.DeleteAsync(10);
        _hallRepoMock.Verify(r => r.DeleteAsync(10), Times.Once);
    }
}