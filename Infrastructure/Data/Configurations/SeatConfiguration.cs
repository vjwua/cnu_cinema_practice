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

        // Додано: індекс для швидкого пошуку доступних місць
        builder.HasIndex(x => new { x.SessionId, x.IsAvailable });

        builder.Property(x => x.RowNum)
            .IsRequired();

        builder.Property(x => x.SeatNum)
            .IsRequired();

        builder.Property(x => x.SeatType)
            .HasConversion<byte>()
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(x => x.AddedPrice)
            .HasPrecision(6, 2)
            .IsRequired();

        builder.Property(x => x.IsAvailable)
            .IsRequired();

        // Заборона каскадного видалення
        builder.HasOne(x => x.Session)
            .WithMany(s => s.Seats)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Restrict);

        // захист від некоректних enum-значень
        builder.HasCheckConstraint(
            "CK_Seats_SeatType",
            "SeatType IN (0, 1, 2, 3)");

        // валідація координат місць (0-15 для 16x16)
        builder.HasCheckConstraint(
            "CK_Seats_RowNum",
            "RowNum >= 0 AND RowNum < 16");

        builder.HasCheckConstraint(
            "CK_Seats_SeatNum",
            "SeatNum >= 0 AND SeatNum < 16");

        // AddedPrice не може бути від'ємною
        builder.HasCheckConstraint(
            "CK_Seats_AddedPrice",
            "AddedPrice >= 0");
    }
}