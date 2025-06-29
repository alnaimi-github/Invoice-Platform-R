using System.ComponentModel.DataAnnotations;

namespace InvoiceProcessing.API.DTOs.Invoices;

public sealed record CreateInvoiceDto(
    [property: Required] IFormFile InvoiceFile,
    [property: Required] Guid CustomerId
);