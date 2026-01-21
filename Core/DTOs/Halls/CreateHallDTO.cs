using Core.Mapping;

namespace Core.DTOs.Halls;

public class CreateHallDTO
{
    public int Name { get; set; }
    public SeatLayoutMap SeatLayout { get; set; }
}