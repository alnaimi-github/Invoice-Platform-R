namespace InvoiceProcessing.API.Models;

public class InvoiceTax : BaseEntity
{
    public string TaxCategoryId { get; set; } = string.Empty;
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TaxRate { get; set; }
    public string TaxSchemeId { get; set; } = "VAT";
    
    // Foreign Key
    public Guid InvoiceId { get; set; }
    
    // Navigation Property
    public virtual Invoice Invoice { get; set; } = null!;
}