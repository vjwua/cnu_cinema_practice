using Core.Enums;

namespace cnu_cinema_practice.ViewModels.Account;

public class ProfileViewModel
{
    public string Email { get; set; } = null!;
    public string? FullName { get; set; }
    public bool IsAdmin { get; set; }

    public int TotalOrdersCount { get; set; }

    public DateTime? FilterFrom { get; set; }
    public DateTime? FilterTo { get; set; }
    public OrderStatus? FilterStatus { get; set; }

    public List<OrderViewModel> Orders { get; set; } = new();
}