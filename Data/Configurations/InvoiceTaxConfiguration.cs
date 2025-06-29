using InvoiceProcessing.API.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceProcessing.API.Data.Configurations;

public sealed class InvoiceTaxConfiguration : IEntityTypeConfiguration<InvoiceTax>
{
    public void Configure(EntityTypeBuilder<InvoiceTax> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TaxableAmount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.TaxRate).HasColumnType("decimal(5,2)");

        builder.HasOne(e => e.Invoice)
            .WithMany(i => i.InvoiceTaxes)
            .HasForeignKey(e => e.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}