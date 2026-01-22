using Core.DTOs;

namespace Core.DTOs.Halls;

public class CreateHallDTO
{
    public string Name { get; set; }
    public byte Rows { get; set; }
    public byte Columns { get; set; }
    public byte[,] SeatLayout { get; set; } = null!;
}