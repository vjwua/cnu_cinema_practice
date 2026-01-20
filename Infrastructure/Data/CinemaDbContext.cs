using Infrastructure.Identity;

namespace Infrastructure.Data;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Core.Entities;


public class CinemaDbContext
    : IdentityDbContext<ApplicationUser>
{
    public CinemaDbContext(DbContextOptions<CinemaDbContext> options)
        : base(options) { }

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<MovieDirector> MovieDirectors => Set<MovieDirector>();
    public DbSet<MovieActor> MovieActors => Set<MovieActor>();
    public DbSet<Person> People => Set<Person>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(
            typeof(CinemaDbContext).Assembly
        );

        // зв'язок Order → Identity User (без навігації в Core)
        builder.Entity<Order>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // зв'язок Ticket
        builder.Entity<Ticket>(entity =>
        {
            entity
                .HasOne(t => t.Order)
                .WithMany(o => o.Tickets)
                .HasForeignKey(t => t.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne(t => t.Seat)
                .WithMany()
                .HasForeignKey(t => t.SeatId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // зв'язок Payment → Order
        builder.Entity<Payment>()
            .HasOne(p => p.Order)
            .WithOne(o => o.Payment)
            .HasForeignKey<Payment>(p => p.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        //  зв'язок багато-до-багатьох: Movie ↔ Actor
        builder.Entity<Movie>()
            .HasMany(m => m.Actors)
            .WithMany(a => a.ActedMovies)
            .UsingEntity<MovieActor>(
                right => right
                    .HasOne<Person>()
                    .WithMany()
                    .HasForeignKey(ma => ma.ActorId)
                    .OnDelete(DeleteBehavior.Restrict),
                left => left
                    .HasOne<Movie>()
                    .WithMany()
                    .HasForeignKey(ma => ma.MovieId)
                    .OnDelete(DeleteBehavior.Restrict)
            );

        // зв'язок багато-до-багатьох: Movie ↔ Director
        builder.Entity<Movie>()
            .HasMany(m => m.Directors)
            .WithMany(d => d.DirectedMovies)
            .UsingEntity<MovieDirector>(
                right => right
                    .HasOne<Person>()
                    .WithMany()
                    .HasForeignKey(md => md.DirectorId)
                    .OnDelete(DeleteBehavior.Restrict),
                left => left
                    .HasOne<Movie>()
                    .WithMany()
                    .HasForeignKey(md => md.MovieId)
                    .OnDelete(DeleteBehavior.Restrict)
            );
    }
}
