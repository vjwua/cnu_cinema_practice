using Core.DTOs.Halls;
using Core.DTOs.Seats;

namespace Core.Interfaces.Services;

public interface IHallService
{
    Task<IEnumerable<HallListDTO>> GetAllAsync();
    Task<HallDetailDTO> GetByIdAsync(int hallId);
    Task<HallDetailDTO> CreateAsync(CreateHallDTO hallInfo);
    Task UpdateHallInfo(UpdateHallDTO hallInfo);
    Task UpdateHallDimensions(int hallId, byte rows, byte cols);
    Task SetSeatTypesAsync(int seatId, int seatType);
    Task RemoveAllSeatsAsync(int hallId);
    Task<int> GetSeatCountAsync(int hallId);
    Task<bool> ExistsAsync(int hallId);
    Task DeleteAsync(int hallId);
    Task<IEnumerable<SeatDTO>> GetSeatsByHall(int hallId);
    Task SaveChangesAsync();
    Task CreateSeatAsync(int hallId, SeatDTO seat);
    Task UpdateLayout(int hallId, int r, int c, string layoutString);
    Task DeleteSeat(int seatId);
}