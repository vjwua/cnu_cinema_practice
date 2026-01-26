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
using Core.Validators.Hall;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CinemaDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")
            ));

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<CinemaDbContext>()
            .AddDefaultTokenProviders();

        // Register Repositories
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IHallRepository, HallRepository>();
        services.AddScoped<ISeatRepository, SeatRepository>();

        // Register Services
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IMovieService, MovieService>();
        services.AddScoped<IHallService, HallService>();
        services.AddScoped<ISeatService, SeatService>();

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