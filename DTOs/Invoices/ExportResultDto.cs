namespace InvoiceProcessing.API.DTOs.Invoices;

public sealed record ExportResultDto(
    string FileName,
    string ContentType,
    byte[] Data,
    int RecordCount
);