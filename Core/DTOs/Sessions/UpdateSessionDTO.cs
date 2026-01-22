using Core.Enums;

namespace Core.DTOs.Sessions;

public class UpdateSessionDTO
{
    //це id сутності для оновлення
    public int Id { get; set; }
    public int? MovieId { get; set; }
    public int? HallId { get; set; }
    public DateTime? StartTime { get; set; }
    public decimal? BasePrice { get; set; }
    public MovieFormat? MovieFormat { get; set; }
}