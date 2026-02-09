using cnu_cinema_practice.Controllers;
using Infrastructure;
using Infrastructure.Data.SeedData;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using cnu_cinema_practice.Identity;
using Core.Interfaces;
using Core.Interfaces.Services;
using Infrastructure.Data;
using Infrastructure.Services;

namespace cnu_cinema_practice;

public class Program
{
    public static async Task Main(string[] args)
    {
        PdfSharpCore.Fonts.GlobalFontSettings.FontResolver = new SystemFontResolver();

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.Configure<EmailSettings>(
            builder.Configuration.GetSection("EmailSettings"));

        builder.Services.AddScoped<IEmailService, EmailService>();

        // Add Blazor services
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Add authentication services for Blazor
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddControllersWithViews();
        builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        builder.Services.AddHostedService<ReservationCleanup>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var adminEmail = builder.Configuration["Identity:DefaultAdmin:Email"] ?? "admin@cinema.com";
            var adminPassword = builder.Configuration["Identity:DefaultAdmin:Password"] ?? "Admin123!";
            await IdentitySeed.SeedAsync(roleManager, userManager, adminEmail, adminPassword, logger);
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapControllers();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

        // Map Blazor Root
        app.MapRazorComponents<Components.App>()
            .AddInteractiveServerRenderMode();

        // Маршрут для Admin Area (ВАЖЛИВО: до default route)
        app.MapControllerRoute(
            name: "admin",
            pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}",
            defaults: new { area = "Admin" }
        );

        // Стандартний маршрут для клієнтської частини
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        await app.RunAsync();
    }
}