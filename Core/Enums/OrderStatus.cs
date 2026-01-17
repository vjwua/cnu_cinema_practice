namespace Core.Enums;

public enum OrderStatus : byte
{
    Created   = 0,
    Pending   = 1,
    Paid      = 2,
    Cancelled = 3,
    Refunded  = 4
}