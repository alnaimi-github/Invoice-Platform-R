using System.ComponentModel.DataAnnotations;

namespace InvoiceProcessing.API.Models;

public class InvoiceLine : BaseEntity
{
    public int LineNumber { get; set; }
    
    [Required]
    public string ItemName { get; set; } = string.Empty;
    
    public string? ItemCode { get; set; }
    public string? BuyerItemId { get; set; }
    public decimal Quantity { get; set; }
    public string UnitCode { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TaxRate { get; set; }
    public string TaxCategoryId { get; set; } = string.Empty;
    
    // Foreign Key
    public Guid InvoiceId { get; set; }
    
    // Navigation Property
    public virtual Invoice Invoice { get; set; } = null!;
}