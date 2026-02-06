using System.Net.Mail;
using Core.Constants;
using Core.Interfaces.Services;
using cnu_cinema_practice.ViewModels.Account;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace cnu_cinema_practice.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<AccountController> logger,
    IEmailService emailService) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public Task<IActionResult> Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Task.FromResult<IActionResult>(RedirectToAction("Index", "Home", new { area = "" }));
        }

        ViewData["ReturnUrl"] = returnUrl;
        return Task.FromResult<IActionResult>(View(new LoginViewModel { ReturnUrl = returnUrl }));
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
            return RedirectToLocal(url);
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
    public Task<IActionResult> Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Task.FromResult<IActionResult>(RedirectToAction("Index", "Home", new { area = "" }));
        }

        ViewData["ReturnUrl"] = returnUrl;
        return Task.FromResult<IActionResult>(View(new RegisterViewModel()));
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

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            logger.LogWarning("Password reset requested for non-existent email: {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "No account exists with this email address.");
            return View(model);
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var callbackUrl = Url.Action(
            "ResetPassword",
            "Account",
            new { area = "", email = model.Email, token },
            protocol: Request.Scheme);

        try
        {
            await emailService.SendPasswordResetEmailAsync(model.Email, callbackUrl!);
            logger.LogInformation("Password reset email sent to {Email}", model.Email);
        }
        catch (SmtpException ex)
        {
            logger.LogError(ex, "SMTP error sending email to {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Failed to send email. Please try again later.");
            return View(model);
        }

        return RedirectToAction(nameof(ForgotPasswordConfirmation), "Account", new { area = "" });
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? email, string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("A token is required for password reset.");
        }

        var model = new ResetPasswordViewModel
        {
            Email = email ?? string.Empty,
            Token = token
        };
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return RedirectToAction(nameof(ResetPasswordConfirmation), "Account", new { area = "" });
        }

        var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if (result.Succeeded)
        {
            logger.LogInformation("Password reset successful for {Email}", model.Email);
            return RedirectToAction(nameof(ResetPasswordConfirmation), "Account", new { area = "" });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation()
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