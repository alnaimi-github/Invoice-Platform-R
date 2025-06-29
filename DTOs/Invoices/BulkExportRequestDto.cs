namespace InvoiceProcessing.API.DTOs.Invoices;

public sealed record BulkExportRequestDto(
    ExportFormat Format,
    List<Guid> InvoiceIds,
    bool IncludeLineItems = true,
    bool IncludeTaxDetails = true
);