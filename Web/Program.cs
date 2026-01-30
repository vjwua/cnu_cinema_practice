using Infrastructure;

namespace cnu_cinema_practice;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add Blazor services
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddControllersWithViews();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery(); // Required for Blazor

        // Map Blazor Root
        app.MapRazorComponents<Components.App>()
            .AddInteractiveServerRenderMode();

        // Маршрут для Admin Area (ВАЖЛИВО: до default route)
        app.MapControllerRoute(
            name: "admin",
            pattern: "{controller=Dashboard}/{action=Index}/{id?}",
            defaults: new { area = "Admin" }
        );

        // Стандартний маршрут для клієнтської частини
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}