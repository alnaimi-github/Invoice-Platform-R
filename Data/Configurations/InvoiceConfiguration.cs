using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceProcessing.API.Data.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(e => e.UUID).IsRequired().HasMaxLength(100);
        builder.Property(e => e.CurrencyCode).HasMaxLength(3);
        builder.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.NetAmount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");

        builder.HasOne(e => e.Supplier)
            .WithMany(u => u.SentInvoices)
            .HasForeignKey(e => e.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Customer)
            .WithMany(u => u.ReceivedInvoices)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.InvoiceNumber).IsUnique();
        builder.HasIndex(e => e.UUID).IsUnique();
    }
}