using Core.Enums;

namespace Core.DTOs.Sessions;

public class SessionDetailDTO
{
    public int Id { get; set; }

    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int MovieDurationMinutes { get; set; }

    public string HallName { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }
    public decimal BasePrice { get; set; }
    public MovieFormat MovieFormat { get; set; }

    public List<SeatReservationDTO> Seats { get; set; } = [];
}

// delete when SeatReservationDTO would be implemented
public class SeatReservationDTO
{
    public int SeatId { get; set; } // ID місця
    public byte RowNum { get; set; } // ряд
    public byte SeatNum { get; set; } // номер місця
    public string SeatType { get; set; } = string.Empty; // тип місця (наприклад, стандартне, VIP)
    public decimal AddedPrice { get; set; } // додаткова ціна для цього типу місця

    public bool IsAvailable { get; set; }
}