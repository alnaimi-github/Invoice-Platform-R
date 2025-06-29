using InvoiceProcessing.API.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceProcessing.API.Data.Configurations;

public sealed class VerificationDocumentConfiguration : IEntityTypeConfiguration<VerificationDocument>
{
    public void Configure(EntityTypeBuilder<VerificationDocument> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.DocumentNumber).IsRequired().HasMaxLength(100);
        builder.Property(e => e.S3FileKey).IsRequired().HasMaxLength(500);
        builder.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(255);

        builder.HasOne(e => e.User)
            .WithMany(u => u.VerificationDocuments)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}