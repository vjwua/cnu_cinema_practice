using Core.Entities;
using Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(x => x.Id);

        // ВИПРАВЛЕНО: додано максимальну довжину для UserId
        builder.Property(x => x.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<byte>()
            .HasColumnType("tinyint")
            .HasDefaultValue(OrderStatus.Created)
            .IsRequired();

        builder.HasCheckConstraint(
            "CK_Orders_Status",
            "Status IN (0, 1, 2, 3, 4, 5)");

        // ДОДАНО: індекси для швидкого пошуку
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.CreatedAt });
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.SessionId, x.Status });
    }
}