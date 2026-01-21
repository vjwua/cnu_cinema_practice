using Core.Mapping;

namespace Core.DTOs.Halls;

public class UpdateHallDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public byte[] SeatLayout { get; set; } = null!;
}