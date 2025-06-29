namespace InvoiceProcessing.API.DTOs.Invoices;

    public sealed record InvoiceTaxDto(
        string TaxCategoryId,
        decimal TaxableAmount,
        decimal TaxAmount,
        decimal TaxRate
    );