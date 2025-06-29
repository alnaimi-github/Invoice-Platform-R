using InvoiceProcessing.API.DTOs.Invoices;

namespace InvoiceProcessing.API.Services.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceDto createInvoiceDto, Guid supplierId);
    Task<InvoiceDto> GetInvoiceByIdAsync(Guid invoiceId);
    Task<List<InvoiceDto>> GetInvoicesAsync(Guid? userId = null, int page = 1, int pageSize = 20);
    Task<InvoiceDto> UpdateInvoiceStatusAsync(Guid invoiceId, InvoiceStatus status);
    Task<string> GetInvoiceFileUrlAsync(Guid invoiceId);
    Task<bool> DeleteInvoiceAsync(Guid invoiceId);
}