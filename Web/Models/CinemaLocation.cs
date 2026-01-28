namespace cnu_cinema_practice.Models;

public class CinemaLocation
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public int HallCount { get; set; }
    public double Rating { get; set; }
}
