using InvoiceProcessing.API.Models;

namespace InvoiceProcessing.API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync())
            return; // Data already seeded

        // Create admin user
        var adminUser = new User
        {
            Email = "admin@invoiceprocessing.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            FirstName = "System",
            LastName = "Administrator",
            Role = UserRole.Admin,
            VerificationStatus = VerificationStatus.Approved,
            IsEmailVerified = true,
            CompanyName = "Invoice Processing System"
        };

        // Create sample seller
        var seller = new User
        {
            Email = "seller@company.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Seller123!"),
            FirstName = "Abdullah",
            LastName = "Al-Attas",
            Role = UserRole.Seller,
            VerificationStatus = VerificationStatus.Approved,
            IsEmailVerified = true,
            CompanyName = "مؤسسة عبدالله غازي العطاس للتجارة",
            CommercialRegistrationNumber = "1010244903",
            TaxId = "300099933600003"
        };

        // Create sample buyer
        var buyer = new User
        {
            Email = "buyer@factory.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Buyer123!"),
            FirstName = "Mohammed",
            LastName = "Al-Tamimi",
            Role = UserRole.Buyer,
            VerificationStatus = VerificationStatus.Approved,
            IsEmailVerified = true,
            CompanyName = "مصنع زاويه التميز للصناعه",
            CommercialRegistrationNumber = "1010633374",
            TaxId = "310559313200003"
        };

        context.Users.AddRange(adminUser, seller, buyer);
        await context.SaveChangesAsync();

        // Create sample invoice
        var sampleInvoice = new Invoice
        {
            InvoiceNumber = "45950",
            UUID = "7ec00ef3-40de-40de-8ec2-f3eb98aa3174",
            IssueDate = new DateTime(2024, 6, 13),
            IssueTime = new TimeOnly(9, 50, 11),
            InvoiceTypeCode = "388",
            CurrencyCode = "SAR",
            LineCount = 2,
            TotalAmount = 126.50m,
            TaxAmount = 16.50m,
            NetAmount = 110.00m,
            DiscountAmount = 0.00m,
            Status = InvoiceStatus.Processed,
            SupplierId = seller.Id,
            CustomerId = buyer.Id,
            ICV = 2577,
            QRCode = "Sample QR Code Data"
        };

        // Add sample invoice lines
        var invoiceLines = new List<InvoiceLine>
        {
            new InvoiceLine
            {
                InvoiceId = sampleInvoice.Id,
                LineNumber = 1,
                ItemName = "دسك قص رخام ساده 4.5ب",
                ItemCode = "80190004",
                BuyerItemId = "11390",
                Quantity = 2,
                UnitCode = "حبة",
                UnitPrice = 20,
                LineTotal = 40,
                TaxAmount = 6.00m,
                TaxRate = 15,
                TaxCategoryId = "S"
            },
            new InvoiceLine
            {
                InvoiceId = sampleInvoice.Id,
                LineNumber = 2,
                ItemName = "حجر قص استيل جرندنك 4.5ب115*1*22",
                ItemCode = "80160003",
                BuyerItemId = "11374",
                Quantity = 20,
                UnitCode = "حبة",
                UnitPrice = 3.5m,
                LineTotal = 70,
                TaxAmount = 10.50m,
                TaxRate = 15,
                TaxCategoryId = "S"
            }
        };

        // Add sample tax totals
        var invoiceTax = new InvoiceTax
        {
            InvoiceId = sampleInvoice.Id,
            TaxCategoryId = "S",
            TaxableAmount = 110.00m,
            TaxAmount = 16.50m,
            TaxRate = 15,
            TaxSchemeId = "VAT"
        };

        sampleInvoice.InvoiceLines = invoiceLines;
        sampleInvoice.InvoiceTaxes = new List<InvoiceTax> { invoiceTax };

        context.Invoices.Add(sampleInvoice);
        await context.SaveChangesAsync();
    }
}