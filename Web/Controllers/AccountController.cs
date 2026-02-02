using Core.Constants;
using cnu_cinema_practice.ViewModels.Account;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace cnu_cinema_practice.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<AccountController> logger) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await userManager.GetUserAsync(User);
            if (user != null && await userManager.IsInRoleAsync(user, RoleNames.Admin))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        var url = returnUrl ?? model.ReturnUrl ?? Url.Content("~/");
        ViewData["ReturnUrl"] = url;

        if (!ModelState.IsValid)
            return View(model);

        var result = await signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            isPersistent: model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            logger.LogInformation("User logged in successfully");
            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null || !await userManager.IsInRoleAsync(user, RoleNames.Admin)) return RedirectToLocal(url);
            if (!string.IsNullOrEmpty(url)
                && Url.IsLocalUrl(url)
                && url.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
            {
                return Redirect(url);
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        if (result.IsLockedOut)
        {
            logger.LogWarning("User account locked out: {Email}", model.Email);
            return RedirectToAction(nameof(Lockout), "Account", new { area = "" });
        }

        logger.LogWarning("Failed login attempt for email: {Email}", model.Email);

        ModelState.AddModelError(string.Empty, "Invalid email or password");
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Lockout()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await userManager.GetUserAsync(User);
            if (user != null && await userManager.IsInRoleAsync(user, RoleNames.Admin))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        var url = returnUrl ?? model.ReturnUrl ?? Url.Content("~/");
        ViewData["ReturnUrl"] = url;

        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        await userManager.AddToRoleAsync(user, RoleNames.User);

        await signInManager.SignInAsync(user, isPersistent: false);

        logger.LogInformation("New user registered and signed in");
        return RedirectToLocal(url);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        await signInManager.SignOutAsync();
        logger.LogInformation("User logged out");
        return Redirect("~/");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home", new { area = "" });
    }
}