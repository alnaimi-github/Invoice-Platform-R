using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceProcessing.API.Data.Configurations;

public sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ItemName).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Quantity).HasColumnType("decimal(18,3)");
        builder.Property(e => e.UnitPrice).HasColumnType("decimal(18,3)");
        builder.Property(e => e.LineTotal).HasColumnType("decimal(18,2)");
        builder.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.TaxRate).HasColumnType("decimal(5,2)");

        builder.HasOne(e => e.Invoice)
            .WithMany(i => i.InvoiceLines)
            .HasForeignKey(e => e.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}