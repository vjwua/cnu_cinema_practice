using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class HallConfiguration : IEntityTypeConfiguration<Hall>
{
    public void Configure(EntityTypeBuilder<Hall> builder)
    {
        builder.ToTable("Halls");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.SeatLayout)
            .HasColumnType("binary(64)")
            .HasMaxLength(64)
            .IsRequired();

        builder.HasCheckConstraint(
            "CK_Halls_SeatLayout_Length",
            "DATALENGTH(SeatLayout) = 64");
    }
}