namespace Infrastructure.Data.Configurations;

using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.ToTable("Movies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.PosterUrl)
            .HasMaxLength(500);

        builder.Property(x => x.TrailerUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Country)
            .HasMaxLength(100);

        builder.Property(x => x.ImdbRating)
            .HasPrecision(3, 1);

        builder.Property(x => x.Genre)
            .HasColumnType("bigint");

        builder.Property(x => x.DurationMinutes);
        builder.Property(x => x.AgeLimit);
        builder.Property(x => x.ReleaseDate);

        // ✅ ДОДАТИ: Many-to-Many для Directors
        builder.HasMany(m => m.Directors)
            .WithMany(p => p.DirectedMovies)
            .UsingEntity<Dictionary<string, object>>(
                "MovieDirector",
                j => j.HasOne<Person>()
                    .WithMany()
                    .HasForeignKey("DirectorId")
                    .OnDelete(DeleteBehavior.Restrict),
                j => j.HasOne<Movie>()
                    .WithMany()
                    .HasForeignKey("MovieId")
                    .OnDelete(DeleteBehavior.Restrict),
                j =>
                {
                    j.HasKey("MovieId", "DirectorId");
                    j.ToTable("MovieDirector");
                }
            );

        // ✅ ДОДАТИ: Many-to-Many для Actors
        builder.HasMany(m => m.Actors)
            .WithMany(p => p.ActedInMovies)
            .UsingEntity<Dictionary<string, object>>(
                "MovieActor",
                j => j.HasOne<Person>()
                    .WithMany()
                    .HasForeignKey("ActorId")
                    .OnDelete(DeleteBehavior.Restrict),
                j => j.HasOne<Movie>()
                    .WithMany()
                    .HasForeignKey("MovieId")
                    .OnDelete(DeleteBehavior.Restrict),
                j =>
                {
                    j.HasKey("MovieId", "ActorId");
                    j.ToTable("MovieActor");
                }
            );
    }
}