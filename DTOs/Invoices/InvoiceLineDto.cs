namespace InvoiceProcessing.API.DTOs.Invoices;

    public sealed record InvoiceLineDto(
        int LineNumber,
        string ItemName,
        string? ItemCode,
        decimal Quantity,
        string UnitCode,
        decimal UnitPrice,
        decimal LineTotal,
        decimal TaxAmount,
        decimal TaxRate
    );