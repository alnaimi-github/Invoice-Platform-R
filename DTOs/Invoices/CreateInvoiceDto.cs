using System.ComponentModel.DataAnnotations;

namespace InvoiceProcessing.API.DTOs.Invoices;

public sealed record CreateInvoiceDto
{

    [Required(ErrorMessage = "Invoice file is required.")]
    public IFormFile InvoiceFile { get; init; } = null!;
    [Required(ErrorMessage = "Customer ID is required.")]
    public Guid CustomerId { get; init; }
}