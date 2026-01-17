
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasIndex(x => new { x.OrderId, x.SeatId })
            .IsUnique();

        builder.Property(x => x.TotalPrice)
            .HasPrecision(8, 2);
    }
}
