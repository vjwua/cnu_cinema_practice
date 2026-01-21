namespace Infrastructure.Data.Configurations;

using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class HallConfiguration : IEntityTypeConfiguration<Hall>
{
    public void Configure(EntityTypeBuilder<Hall> builder)
    {
        builder.ToTable("Halls");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Rows)
            .IsRequired();

        builder.Property(x => x.Columns)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasCheckConstraint(
            "CK_Halls_Rows",
            "Rows > 0 AND Rows <= 50");

        builder.HasCheckConstraint(
            "CK_Halls_Columns",
            "Columns > 0 AND Columns <= 50");
    }
}