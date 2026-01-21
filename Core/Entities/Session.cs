using Core.Enums;

namespace Core.Entities;

public class Session
{
    public int Id { get; set; }

    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;

    public int HallId { get; set; }
    public Hall Hall { get; set; } = null!;

    public DateTime StartTime { get; set; }
    public decimal BasePrice { get; set; }
    public MovieFormat MovieFormat { get; set; }

    // public ICollection<Seat> Seats { get; set; } = new List<Seat>()
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}