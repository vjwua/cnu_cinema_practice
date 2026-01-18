using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class MovieActorConfiguration : IEntityTypeConfiguration<MovieActor>
{
    public void Configure(EntityTypeBuilder<MovieActor> builder)
    {
        builder.ToTable("MovieActor");
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.MovieId);
        builder.Property(x => x.ActorId);
        builder.Property(x => x.Role)
            .HasMaxLength(100);
    }
}