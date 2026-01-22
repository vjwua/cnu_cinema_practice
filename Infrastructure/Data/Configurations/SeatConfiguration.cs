
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("Seats");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.HallId, x.RowNum, x.SeatNum })
            .IsUnique();

        builder.Property(x => x.RowNum).IsRequired();
        builder.Property(x => x.SeatNum).IsRequired();

        // Hall → Seats: RESTRICT
        builder.HasOne(x => x.Hall)
            .WithMany(h => h.Seats)
            .HasForeignKey(x => x.HallId)
            .OnDelete(DeleteBehavior.Restrict);

        // SeatType → Seats: RESTRICT
        builder.HasOne(x => x.SeatType)
            .WithMany(st => st.Seats)
            .HasForeignKey(x => x.SeatTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}