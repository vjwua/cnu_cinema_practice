using Core.Entities;

namespace Core.DTOs.Halls;

public class HallDetailDTO
{
    public string Name { get; private set; } = null!;
    public byte[] SeatLayout { get; private set; } = null!;
    public ICollection<Session> Sessions { get; private set; }
}