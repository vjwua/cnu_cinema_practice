namespace Infrastructure.Data.Configurations;

using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SeatTypeConfiguration : IEntityTypeConfiguration<SeatType>
{
    public void Configure(EntityTypeBuilder<SeatType> builder)
    {
        builder.ToTable("SeatTypes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.AddedPrice)
            .HasPrecision(6, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.Description)
            .HasMaxLength(200);

        builder.Property(x => x.ColorCode)
            .HasMaxLength(7);
        
        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}