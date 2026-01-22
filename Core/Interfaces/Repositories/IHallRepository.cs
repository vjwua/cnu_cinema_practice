using Core.Entities;

    
namespace Core.Interfaces.Repositories;
public interface IHallRepository
{
    Task<Hall?> GetByIdAsync(int id);
    Task<Hall?> GetByIdWithSeatsAsync(int id);
    Task<IEnumerable<Hall>> GetAllAsync();
    Task<IEnumerable<Hall>> GetAllWithSeatsAsync();
    Task<Hall> CreateAsync(Hall hall);
    Task UpdateAsync(Hall hall);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    
    Task AddSeatsAsync(int hallId, IEnumerable<Seat> seats);
    Task RemoveAllSeatsAsync(int hallId);
    Task<int> GetSeatCountAsync(int hallId);
}