using InvoiceProcessing.API.Models;

namespace InvoiceProcessing.API.DTOs.Invoices;

public sealed record ExportRequestDto(
    ExportFormat Format,
    DateTime? StartDate,
    DateTime? EndDate,
    InvoiceStatus? Status,
    Guid? SupplierId,
    Guid? CustomerId,
    List<Guid>? InvoiceIds,
    bool IncludeLineItems = true,
    bool IncludeTaxDetails = true
);