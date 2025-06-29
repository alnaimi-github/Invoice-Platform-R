namespace InvoiceProcessing.API.DTOs.Invoices;

public sealed record UpdateInvoiceStatusDto(
    InvoiceStatus Status
);