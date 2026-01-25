namespace Core.DTOs.Orders;

public class CreateOrderDTO
{
    public int SessionId { get; set; }
    public List<int> SeatReservationIds { get; set; } = new();
}