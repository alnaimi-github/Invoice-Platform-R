using System.ComponentModel.DataAnnotations;

namespace InvoiceProcessing.API.Models;

public class Invoice : BaseEntity
{
    [Required]
    public string InvoiceNumber { get; set; } = string.Empty;
    
    [Required]
    public string UUID { get; set; } = string.Empty;
    
    public DateTime IssueDate { get; set; }
    public TimeOnly IssueTime { get; set; }
    public string InvoiceTypeCode { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "SAR";
    public int LineCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public string? DigitalSignature { get; set; }
    public string? QRCode { get; set; }
    public string? PIH { get; set; }
    public int ICV { get; set; }
    public string? S3FileKey { get; set; }
    public string? OriginalFileName { get; set; }
    public long? FileSizeBytes { get; set; }
    
    // Foreign Keys
    public Guid SupplierId { get; set; }
    public Guid CustomerId { get; set; }
    
    // Navigation Properties
    public virtual User Supplier { get; set; } = null!;
    public virtual User Customer { get; set; } = null!;
    public virtual ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
    public virtual ICollection<InvoiceTax> InvoiceTaxes { get; set; } = new List<InvoiceTax>();
}