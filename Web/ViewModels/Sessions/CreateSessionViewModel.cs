using System.ComponentModel.DataAnnotations;
using Core.Enums;

namespace cnu_cinema_practice.ViewModels.Sessions;

public class CreateSessionViewModel
{
    [Required(ErrorMessage = "Please select a movie")]
    [Display(Name = "Movie")]
    public int MovieId { get; set; }

    [Required(ErrorMessage = "Please select a hall")]
    [Display(Name = "Hall")]
    public int HallId { get; set; }

    [Required(ErrorMessage = "Please specify start time")]
    [Display(Name = "Start Time")]
    [DataType(DataType.DateTime)]
    public DateTime StartTime { get; set; } = DateTime.Now.AddHours(1);

    [Required(ErrorMessage = "Please specify base price")]
    [Display(Name = "Base Price")]
    [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10000")]
    [DataType(DataType.Currency)]
    public decimal BasePrice { get; set; }

    [Required(ErrorMessage = "Please select movie format")]
    [Display(Name = "Format")]
    public MovieFormat MovieFormat { get; set; }
}