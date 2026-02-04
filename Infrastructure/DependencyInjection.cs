using Infrastructure.Identity;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Services;
using Core.Validators.Sessions;
using Core.Validators.Movies;
using Core.Validators.Hall;
using Core.DTOs.Sessions;
using Core.DTOs.Movies;
using Core.DTOs.Halls;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddDbContext<CinemaDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")
            ));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<CinemaDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";

            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;

            options.Cookie.HttpOnly = true;

            options.Cookie.SecurePolicy = environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;

            options.Cookie.SameSite = SameSiteMode.Strict;
        });

        // Register Repositories
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IHallRepository, HallRepository>();
        services.AddScoped<ISeatRepository, SeatRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ISeatReservationRepository, SeatReservationRepository>();

        // Register Services
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IMovieService, MovieService>();
        services.AddScoped<IHallService, HallService>();
        services.AddScoped<ISeatService, SeatService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();

        // Register Validators
        // Session Validators
        services.AddScoped<IValidator<CreateSessionDTO>, CreateSessionDtoValidator>();
        services.AddScoped<IValidator<UpdateSessionDTO>, UpdateSessionDtoValidator>();
        services.AddScoped<SessionBusinessValidator>();
        services.AddScoped<CreateSessionBusinessValidator>();
        services.AddScoped<UpdateSessionBusinessValidator>();

        // Movie Validators
        services.AddScoped<IValidator<CreateMovieDTO>, CreateMovieDTOValidator>();
        services.AddScoped<IValidator<UpdateMovieDTO>, UpdateMovieDTOValidator>();

        // Hall Validators
        services.AddScoped<IValidator<CreateHallDTO>, CreateHallDTOValidator>();
        services.AddScoped<IValidator<UpdateHallDTO>, UpdateHallDTOValidator>();

        return services;
    }
}