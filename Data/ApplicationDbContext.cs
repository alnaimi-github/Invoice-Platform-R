namespace InvoiceProcessing.API.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceLine> InvoiceLines { get; set; }
    public DbSet<InvoiceTax> InvoiceTaxes { get; set; }
    public DbSet<VerificationDocument> VerificationDocuments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global Query Filters for Soft Delete
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Invoice>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<InvoiceLine>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<InvoiceTax>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<VerificationDocument>().HasQueryFilter(e => !e.IsDeleted);
    }
}