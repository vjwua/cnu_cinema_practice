using Core.Entities;
using Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(8, 2)
            .IsRequired();

        builder.Property(x => x.PaymentMethod)
            .HasConversion<byte>()
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(x => x.PaidAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();


        // 🛡 захист від некоректних значень enum
        builder.HasCheckConstraint(
            "CK_Payments_PaymentMethod",
            "PaymentMethod IN (0, 1, 2, 3)");
    }
}