
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.PosterUrl)
            .HasMaxLength(500);

        builder.Property(x => x.TrailerUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Director)
            .HasMaxLength(200);

        builder.Property(x => x.Country)
            .HasMaxLength(100);

        builder.Property(x => x.ImdbRating)
            .HasPrecision(3, 1);
    }
}
