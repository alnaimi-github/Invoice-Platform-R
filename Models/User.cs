using System.ComponentModel.DataAnnotations;

namespace InvoiceProcessing.API.Models;

public class User : BaseEntity
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; } = UserRole.Buyer;
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
    public bool IsEmailVerified { get; set; } = false;
    public bool IsPhoneVerified { get; set; } = false;
    public string? CommercialRegistrationNumber { get; set; }
    public string? TaxId { get; set; }
    public string? CompanyName { get; set; }
    
    // Navigation properties
    public virtual ICollection<Invoice> SentInvoices { get; set; } = new List<Invoice>();
    public virtual ICollection<Invoice> ReceivedInvoices { get; set; } = new List<Invoice>();
    public virtual ICollection<VerificationDocument> VerificationDocuments { get; set; } = new List<VerificationDocument>();
}