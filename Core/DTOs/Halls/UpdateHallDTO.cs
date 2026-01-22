using Core.DTOs;

namespace Core.DTOs.Halls;

public class UpdateHallDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    //public byte Rows { get; set; }
    // public byte Columns { get; set; }
    public byte[,]? SeatLayout { get; set; }
}