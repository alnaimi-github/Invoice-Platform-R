using InvoiceProcessing.API.DTOs.Users;

namespace InvoiceProcessing.API.DTOs.Invoices;

    public sealed record InvoiceDto(
        Guid Id,
        string InvoiceNumber,
        string UUID,
        DateTime IssueDate,
        TimeOnly IssueTime,
        string InvoiceTypeCode,
        string CurrencyCode,
        int LineCount,
        decimal TotalAmount,
        decimal TaxAmount,
        decimal NetAmount,
        decimal DiscountAmount,
        InvoiceStatus Status,
        string? QRCode,
        string? OriginalFileName,
        DateTime CreatedAt,
        UserSummaryDto Supplier,
        UserSummaryDto Customer,
        List<InvoiceLineDto> InvoiceLines,
        List<InvoiceTaxDto> InvoiceTaxes
    );