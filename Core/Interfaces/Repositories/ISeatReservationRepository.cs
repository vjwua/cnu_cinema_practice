using Core.Entities;

namespace Core.Interfaces.Repositories;

public interface ISeatReservationRepository
{
    Task<List<SeatReservation>> GetByIdsAsync(List<int> ids);
    Task MarkAsSoldAsync(List<int> ids);
}