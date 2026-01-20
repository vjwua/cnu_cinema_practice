using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

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

        builder.HasCheckConstraint(
            "CK_Movies_ImdbRating",
            "ImdbRating IS NULL OR (ImdbRating >= 0.0 AND ImdbRating <= 10.0)");

        builder.Property(x => x.Genre)
            .HasColumnType("bigint");

        builder.Property(x => x.DurationMinutes)
            .IsRequired();

        builder.HasCheckConstraint(
            "CK_Movies_DurationMinutes",
            "DurationMinutes > 0 AND DurationMinutes <= 600"); // макс 10 годин

        builder.Property(x => x.AgeLimit)
            .IsRequired();
        
        builder.Property(x => x.ReleaseDate)
            .IsRequired();
    }
}