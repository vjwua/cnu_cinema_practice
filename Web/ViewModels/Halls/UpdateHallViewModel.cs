using System.ComponentModel.DataAnnotations;

namespace cnu_cinema_practice.ViewModels.Halls;

public class UpdateHallViewModel
{
    [Required(ErrorMessage = "Hall id must not be empty")]
    public int Id { get; set; }
    
    public string? Name { get; set; }
    public byte[,]? SeatLayout { get; set; }
}