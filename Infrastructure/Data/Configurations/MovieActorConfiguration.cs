using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class MovieActorConfiguration : IEntityTypeConfiguration<MovieActor>
{
    public void Configure(EntityTypeBuilder<MovieActor> builder)
    {
        builder.ToTable("MovieActors");
        
        builder.HasKey(x => x.Id);

        // Композитний унікальний індекс для запобігання дублікатів
        builder.HasIndex(x => new { x.MovieId, x.ActorId })
            .IsUnique();
        
        builder.Property(x => x.MovieId)
            .IsRequired();
        
        builder.Property(x => x.ActorId)
            .IsRequired();
        
        builder.Property(x => x.Role)
            .HasMaxLength(100)
            .IsRequired(false);
    }
}