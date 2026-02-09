using Core.DTOs.Seats;
using Core.Entities;

namespace Core.Interfaces.Services;

public interface ISeatService
{
    Task<SeatDTO> GetByIdAsync(int id);
    Task<IEnumerable<SeatDTO>> GetBySessionIdAsync(int sessionId);
    Task<IEnumerable<SeatDTO>> GetAvailableSeatsAsync(int sessionId);
    Task<bool> ReserveSeatAsync(SeatDTO seat, int sessionId, decimal price, string? userId = null);
    Task<int?> GetReservationIdAsync(int seatId, int sessionId);
    Task<bool> IsSeatAvailableAsync(SeatDTO seat, int sessionId);
    Task<IEnumerable<SeatType>> GetSeatTypesAsync();
}