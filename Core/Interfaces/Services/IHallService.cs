using Core.DTOs.Halls;

namespace Core.Interfaces.Services;

public interface IHallService
{
    Task<IEnumerable<HallListDTO>> GetAllAsync();
    Task<HallDetailDTO> GetByIdAsync(int hallId);
    Task<HallDetailDTO> CreateAsync(CreateHallDTO hallInfo);
    Task UpdateHallInfo(UpdateHallDTO hallInfo);
    Task RemoveAllSeatsAsync(int hallId);
    Task<int> GetSeatCountAsync(int hallId);
    Task<bool> ExistsAsync(int hallId);
    Task DeleteAsync(int hallId);
}