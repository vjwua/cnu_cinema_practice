using Core.Entities;

namespace Core.Interfaces.Repositories;

public interface ISeatRepository
{
    Task<Seat?> GetByIdAsync(int id);
    Task<IEnumerable<Seat>> GetBySessionIdAsync(int sessionId);
    Task<IEnumerable<Seat>> GetAvailableSeatsAsync(int sessionId); // перегляд доступних місць
    Task<bool> ReserveSeatAsync(int seatId); // для покупки
    Task<bool> IsSeatAvailableAsync(int seatId);
}