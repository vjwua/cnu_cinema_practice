using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class MovieDirectorConfiguration : IEntityTypeConfiguration<MovieDirector>
{
    public void Configure(EntityTypeBuilder<MovieDirector> builder)
    {
        builder.ToTable("MovieDirectors");
        
        builder.HasKey(x => x.Id);

        // Композитний унікальний індекс для запобігання дублікатів
        builder.HasIndex(x => new { x.MovieId, x.DirectorId })
            .IsUnique();

        builder.Property(x => x.MovieId)
            .IsRequired();
        
        builder.Property(x => x.DirectorId)
            .IsRequired();
    }
}