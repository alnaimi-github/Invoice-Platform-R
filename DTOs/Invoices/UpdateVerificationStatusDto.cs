namespace InvoiceProcessing.API.DTOs.Invoices;

public sealed record UpdateVerificationStatusDto(
    VerificationStatus Status
);