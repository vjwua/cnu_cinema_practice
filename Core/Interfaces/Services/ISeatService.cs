using Core.DTOs.Seats;

namespace Core.Interfaces.Services;

public interface ISeatService
{
    Task<SeatDTO> GetByIdAsync(int id);
    Task<IEnumerable<SeatDTO>> GetBySessionIdAsync(int sessionId);
    Task<IEnumerable<SeatDTO>> GetAvailableSeatsAsync(int sessionId);
    Task<bool> ReserveSeatAsync(SeatDTO seat, int sessionId);
    Task<bool> IsSeatAvailableAsync(SeatDTO seat, int sessionId);
}