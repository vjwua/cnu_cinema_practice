using Core.Entities;
using Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("Seats");

        builder.HasKey(x => x.Id);

        // унікальність місця в межах сесії
        builder.HasIndex(x => new { x.SessionId, x.RowNum, x.SeatNum })
            .IsUnique();

        builder.Property(x => x.RowNum)
            .IsRequired();

        builder.Property(x => x.SeatNum)
            .IsRequired();

        // 🔥 SeatType → tinyint
        builder.Property(x => x.SeatType)
            .HasConversion<byte>()
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(x => x.AddedPrice)
            .HasPrecision(6, 2)
            .IsRequired();

        builder.Property(x => x.IsAvailable)
            .IsRequired();

        builder.HasOne(x => x.Session)
            .WithMany(s => s.Seats)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // 🛡 захист від некоректних enum-значень
        builder.HasCheckConstraint(
            "CK_Seats_SeatType",
            "SeatType IN (0, 1, 2, 3)");
    }
}