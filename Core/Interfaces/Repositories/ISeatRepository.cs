using Core.Entities;

namespace Core.Interfaces.Repositories;

public interface ISeatRepository
{
    Task<Seat?> GetByIdAsync(int id);
    Task<IEnumerable<Seat>> GetBySessionIdAsync(int sessionId);
    Task<IEnumerable<Seat>> GetAvailableSeatsAsync(int sessionId); // перегляд доступних місць
    Task<bool> ReserveSeatAsync(int seatId, int sessionId); // для покупки
    Task<bool> IsSeatAvailableAsync(int seatId, int sessionId);
    Task<IEnumerable<Seat>> GetByHallId(int hallId);
    Task SetSeatTypeAsync(int seatId, int seatType);
    Task CreateAsync(Seat seat);
    Task UpdateHallLayout(int hallId, int r, int c, string ls);
    Task DeleteAsync(int seatId);
    Task<IEnumerable<SeatType>> GetSeatTypesAsync();
}