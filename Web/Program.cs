using Core.Interfaces.Services;
using Core.Mappers;
using Core.Services;
using Infrastructure;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Interfaces;
using FluentValidation;
using Core.DTOs.Sessions;
using Core.Validators.Sessions;

namespace cnu_cinema_practice;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();
        builder.Services.AddInfrastructure(builder.Configuration);


        builder.Services.AddScoped<ISessionRepository, SessionRepository>();
        builder.Services.AddScoped<ISessionService, SessionService>();
        builder.Services.AddAutoMapper(typeof(SessionProfile));
        builder.Services.AddScoped<IValidator<CreateSessionDTO>, CreateSessionDtoValidator>();
        builder.Services.AddScoped<IValidator<UpdateSessionDTO>, UpdateSessionDtoValidator>();
        builder.Services.AddScoped<SessionBusinessValidator>();
        builder.Services.AddScoped<CreateSessionBusinessValidator>();
        builder.Services.AddScoped<UpdateSessionBusinessValidator>();


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

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}