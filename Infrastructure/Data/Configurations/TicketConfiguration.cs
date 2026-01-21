
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.OrderId, x.SeatReservationId })
            .IsUnique();

        builder.Property(x => x.Price)
            .HasPrecision(8, 2)
            .IsRequired();

        // Order → Tickets: RESTRICT
        builder.HasOne(t => t.Order)
            .WithMany(o => o.Tickets)
            .HasForeignKey(t => t.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // SeatReservation → Ticket: RESTRICT
        builder.HasOne(t => t.SeatReservation)
            .WithOne(sr => sr.Ticket)
            .HasForeignKey<Ticket>(t => t.SeatReservationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}