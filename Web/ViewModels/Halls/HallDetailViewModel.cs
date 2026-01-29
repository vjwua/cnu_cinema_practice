using System.ComponentModel.DataAnnotations;
using Core.Entities;

namespace cnu_cinema_practice.ViewModels.Halls;

public class HallDetailViewModel
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Hall must have a name")]
    [Display(Name = "Hall name")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "Hall must have rows")]
    [Display(Name = "Row count")]
    [Range((byte)1, (byte) 50, ErrorMessage = "Hall must have 1-50 rows")]
    public byte Rows { get; set; }
    
    [Required(ErrorMessage = "Hall must have columns")]
    [Display(Name = "Column count")]
    [Range((byte)1, (byte) 50, ErrorMessage = "Hall must have 1-50 columns")]
    public byte Columns { get; set; }
}