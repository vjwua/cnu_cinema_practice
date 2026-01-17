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

        // 🔥 16x16, 2 біти на комірку = 64 байти
        builder.Property(x => x.SeatLayout)
            .HasColumnType("binary(64)")
            .HasMaxLength(64)
            .IsRequired();

        // захист від кривих даних
        builder.HasCheckConstraint(
            "CK_Halls_SeatLayout_Length",
            "DATALENGTH(SeatLayout) = 64");
    }
}