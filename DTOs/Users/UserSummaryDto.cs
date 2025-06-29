namespace InvoiceProcessing.API.DTOs.Users;

public sealed record UserSummaryDto(
    Guid Id,
    string CompanyName,
    string TaxId,
    string Email
);