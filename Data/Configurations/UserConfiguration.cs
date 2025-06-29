using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceProcessing.API.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(255);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.CompanyName).HasMaxLength(255);
        builder.Property(e => e.CommercialRegistrationNumber).HasMaxLength(50);
        builder.Property(e => e.TaxId).HasMaxLength(50);
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.CommercialRegistrationNumber).IsUnique();
        builder.HasIndex(e => e.TaxId).IsUnique();
    }
}