
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("Sessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BasePrice)
            .HasPrecision(8, 2)
            .IsRequired();

        builder.Property(x => x.MovieFormat)
            .HasConversion<byte>()
            .HasColumnType("tinyint")
            .IsRequired();

        builder.HasOne(x => x.Movie)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Hall)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.HallId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}