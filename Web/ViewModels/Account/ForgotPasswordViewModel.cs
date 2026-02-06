using System.ComponentModel.DataAnnotations;

namespace cnu_cinema_practice.ViewModels.Account;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [Display(Name = "Email")]
    public string Email { get; init; } = string.Empty;
}
