namespace Core.Entities;

public class SeatType
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal AddedPrice { get; set; }
    public string? Description { get; set; }
    public string? ColorCode { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
}