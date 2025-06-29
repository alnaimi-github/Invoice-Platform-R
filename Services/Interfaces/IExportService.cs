using InvoiceProcessing.API.DTOs.Invoices;

namespace InvoiceProcessing.API.Services.Interfaces;

public interface IExportService
{
    Task<ExportResultDto> ExportInvoicesAsync(ExportRequestDto request, Guid? userId = null);
    Task<ExportResultDto> ExportSingleInvoiceAsync(Guid invoiceId, ExportFormat format);
    Task<byte[]> GenerateInvoiceXmlAsync(Guid invoiceId);
    Task<byte[]> GenerateInvoiceExcelAsync(List<Guid> invoiceIds);
    Task<byte[]> GenerateInvoiceCsvAsync(List<Guid> invoiceIds);
    Task<byte[]> GenerateInvoicePdfAsync(Guid invoiceId);
    Task<byte[]> GenerateInvoiceJsonAsync(List<Guid> invoiceIds);
}