using Core.Entities;

namespace Core.DTOs.Halls;

public class HallDetailDTO
{
    public string Name { get; private set; } = null!;
    public byte Rows { get; set; }
    public byte Columns { get; set; } 
    public ICollection<Seat> Seats { get; private set; }
    public ICollection<Session> Sessions { get; private set; }
}