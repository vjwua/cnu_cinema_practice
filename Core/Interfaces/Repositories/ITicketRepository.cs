using Core.Entities;

namespace Core.Interfaces.Repositories;

public interface ITicketRepository
{
    Task<Ticket?> GetForScanAsync(int ticketId);
    Task SaveChangesAsync();
}