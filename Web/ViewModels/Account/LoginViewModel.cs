using System.ComponentModel.DataAnnotations;

namespace cnu_cinema_practice.ViewModels.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [Display(Name = "Email")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; init; } = string.Empty;

    [Display(Name = "Remember me")] public bool RememberMe { get; init; }

    public string? ReturnUrl { get; set; }
}