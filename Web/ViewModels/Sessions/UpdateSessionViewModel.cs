using System.ComponentModel.DataAnnotations;
using Core.Enums;

namespace cnu_cinema_practice.ViewModels.Sessions;

public class UpdateSessionViewModel
{
    public int Id { get; set; }

    [Display(Name = "Movie")] public int? MovieId { get; set; }

    [Display(Name = "Hall")] public int? HallId { get; set; }

    [Display(Name = "Start Time")]
    [DataType(DataType.DateTime)]
    public DateTime? StartTime { get; set; }

    [Display(Name = "Base Price")]
    [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10000")]
    [DataType(DataType.Currency)]
    public decimal? BasePrice { get; set; }

    [Display(Name = "Format")] public MovieFormat? MovieFormat { get; set; }
}