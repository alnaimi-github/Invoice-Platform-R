namespace InvoiceProcessing.API.Services.Interfaces;

public interface IXmlParserService
{
    Task<Invoice> ParseInvoiceFromXmlAsync(Stream xmlStream, Guid supplierId, Guid customerId);
    Task<bool> ValidateXmlStructureAsync(Stream xmlStream);
    Task<string> ExtractDigitalSignatureAsync(Stream xmlStream);
    Task<string> ExtractQRCodeAsync(Stream xmlStream);
}