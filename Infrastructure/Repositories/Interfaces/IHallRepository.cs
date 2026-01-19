using Core.Entities;
using Core.Mapping;

namespace Infrastructure.Repositories.Interfaces;

public interface IHallRepository
{
    Task<Hall?> GetByIdAsync(int id);
    Task<Hall?> GetByIdWithLayoutAsync(int id);
    Task<IEnumerable<Hall>> GetAllAsync();
    Task<Hall> CreateAsync(string name, SeatLayoutMap layout);
    Task UpdateLayoutAsync(int hallId, Action<SeatLayoutMap> updateAction);
    Task<bool> ExistsAsync(int id);
}