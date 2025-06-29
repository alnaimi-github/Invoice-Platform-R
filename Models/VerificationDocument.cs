using System.ComponentModel.DataAnnotations;

namespace InvoiceProcessing.API.Models;

public class VerificationDocument : BaseEntity
{
    public DocumentType DocumentType { get; set; }
    
    [Required]
    public string DocumentNumber { get; set; } = string.Empty;
    
    [Required]
    public string S3FileKey { get; set; } = string.Empty;
    
    [Required]
    public string OriginalFileName { get; set; } = string.Empty;
    
    public long FileSizeBytes { get; set; }
    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
    public string? ReviewNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedBy { get; set; }
    
    // Foreign Key
    public Guid UserId { get; set; }
    
    // Navigation Property
    public virtual User User { get; set; } = null!;
}