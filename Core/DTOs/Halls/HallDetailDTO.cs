using Core.Entities;

namespace Core.DTOs.Halls;

public class HallDetailDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public byte Rows { get; set; }
    public byte Columns { get; set; } 
    public ICollection<Seat> Seats { get; set; }
    public ICollection<Session> Sessions { get; set; }
}