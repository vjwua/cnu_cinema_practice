using System.Net.Mail;
using Core.Constants;
using Core.Interfaces.Services;
using cnu_cinema_practice.ViewModels.Account;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using cnu_cinema_practice.Components.Pages.Account;
using Microsoft.AspNetCore.Http.HttpResults;

namespace cnu_cinema_practice.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<AccountController> logger,
    IEmailService emailService) : Controller
{
    private IEnumerable<string> GetModalStateErrors()
    {
        return ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
    }
    [HttpGet]
    [AllowAnonymous]
    public IResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Results.Redirect(Url.Action("Index", "Home", new { area = "" })!);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return new RazorComponentResult<Login>(new { Model = new LoginViewModel { ReturnUrl = returnUrl } });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        var url = returnUrl ?? model.ReturnUrl ?? Url.Content("~/");
        ViewData["ReturnUrl"] = url;

        if (!ModelState.IsValid)
            return new RazorComponentResult<Login>(new { Model = model, Errors = GetModalStateErrors() });

        var result = await signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            isPersistent: model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            logger.LogInformation("User logged in successfully");
            return Results.Redirect(url);
        }

        if (result.IsLockedOut)
        {
            logger.LogWarning("User account locked out: {Email}", model.Email);
            return Results.Redirect(Url.Action(nameof(Lockout), "Account", new { area = "" })!);
        }

        logger.LogWarning("Failed login attempt for email: {Email}", model.Email);

        ModelState.AddModelError(string.Empty, "Invalid email or password");
        return new RazorComponentResult<Login>(new { Model = model, Errors = GetModalStateErrors() });
    }

    [HttpGet]
    [AllowAnonymous]
    public IResult Lockout()
    {
        return new RazorComponentResult<Lockout>();
    }

    [HttpGet]
    [AllowAnonymous]
    public IResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Results.Redirect(Url.Action("Index", "Home", new { area = "" })!);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return new RazorComponentResult<Register>(new { Model = new RegisterViewModel { ReturnUrl = returnUrl } });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        var url = returnUrl ?? model.ReturnUrl ?? Url.Content("~/");
        ViewData["ReturnUrl"] = url;

        if (!ModelState.IsValid)
            return new RazorComponentResult<Register>(new { Model = model, Errors = GetModalStateErrors() });

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
            return new RazorComponentResult<Register>(new { Model = model, Errors = GetModalStateErrors() });
        }

        await userManager.AddToRoleAsync(user, RoleNames.User);

        await signInManager.SignInAsync(user, isPersistent: false);

        logger.LogInformation("New user registered and signed in");
        return Results.Redirect(url);
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
    public IResult AccessDenied()
    {
        return new RazorComponentResult<AccessDenied>();
    }

    [HttpGet]
    [AllowAnonymous]
    public IResult ForgotPassword()
    {
        return new RazorComponentResult<ForgotPassword>(new { Model = new ForgotPasswordViewModel() });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return new RazorComponentResult<ForgotPassword>(new { Model = model, Errors = GetModalStateErrors() });

        var user = await userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            logger.LogWarning("Password reset requested for non-existent email: {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "No account exists with this email address.");
            return new RazorComponentResult<ForgotPassword>(new { Model = model, Errors = GetModalStateErrors() });
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
            return new RazorComponentResult<ForgotPassword>(new { Model = model, Errors = GetModalStateErrors() });
        }

        return Results.Redirect(Url.Action(nameof(ForgotPasswordConfirmation), "Account", new { area = "" })!);
    }

    [HttpGet]
    [AllowAnonymous]
    public IResult ForgotPasswordConfirmation()
    {
        return new RazorComponentResult<ForgotPasswordConfirmation>();
    }

    [HttpGet]
    [AllowAnonymous]
    public IResult ResetPassword(string? email, string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return Results.BadRequest("A token is required for password reset.");
        }

        var model = new ResetPasswordViewModel
        {
            Email = email ?? string.Empty,
            Token = token
        };
        return new RazorComponentResult<ResetPassword>(new { Model = model });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return new RazorComponentResult<ResetPassword>(new { Model = model, Errors = GetModalStateErrors() });

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return Results.Redirect(Url.Action(nameof(ResetPasswordConfirmation), "Account", new { area = "" })!);
        }

        var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if (result.Succeeded)
        {
            logger.LogInformation("Password reset successful for {Email}", model.Email);
            return Results.Redirect(Url.Action(nameof(ResetPasswordConfirmation), "Account", new { area = "" })!);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return new RazorComponentResult<ResetPassword>(new { Model = model, Errors = GetModalStateErrors() });
    }

    [HttpGet]
    [AllowAnonymous]
    public IResult ResetPasswordConfirmation()
    {
        return new RazorComponentResult<ResetPasswordConfirmation>();
    }

}