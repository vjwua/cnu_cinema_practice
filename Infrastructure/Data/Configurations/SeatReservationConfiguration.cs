
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SeatReservationConfiguration : IEntityTypeConfiguration<SeatReservation>
{
    public void Configure(EntityTypeBuilder<SeatReservation> builder)
    {
        builder.ToTable("SeatReservations");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.SessionId, x.SeatId })
            .IsUnique();

        builder.Property(x => x.Status)
            .HasConversion<byte>()
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(x => x.Price)
            .HasPrecision(8, 2)
            .IsRequired();

        // Session → SeatReservations: RESTRICT
        builder.HasOne(x => x.Session)
            .WithMany(s => s.SeatReservations)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seat → SeatReservations: RESTRICT
        builder.HasOne(x => x.Seat)
            .WithMany(s => s.Reservations)
            .HasForeignKey(x => x.SeatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasCheckConstraint(
            "CK_SeatReservations_Status",
            "Status IN (1, 2)");
    }
}