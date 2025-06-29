using AutoMapper;
using InvoiceProcessing.API.DTOs.Invoices;
using InvoiceProcessing.API.Services.Interfaces;

namespace InvoiceProcessing.API.Services;

public class InvoiceService : IInvoiceService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;
    private readonly IXmlParserService _xmlParserService;

    public InvoiceService(
        ApplicationDbContext context,
        IMapper mapper,
        IFileStorageService fileStorageService,
        IXmlParserService xmlParserService)
    {
        _context = context;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
        _xmlParserService = xmlParserService;
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceDto createInvoiceDto, Guid supplierId)
    {
        // Validate file type
        var fileName = createInvoiceDto.InvoiceFile.FileName;
        if (!fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) &&
            !fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only XML and PDF files are supported");
        }

        await using var fileStream = createInvoiceDto.InvoiceFile.OpenReadStream();

        // For PDF files, we assume they contain embedded XML (common in e-invoicing)
        // In a real implementation, you'd extract XML from PDF
        Stream xmlStream = fileStream;

        // Validate XML structure
        if (!await _xmlParserService.ValidateXmlStructureAsync(xmlStream))
        {
            throw new InvalidOperationException("Invalid invoice XML structure");
        }

        // Parse invoice from XML
        var invoice = await _xmlParserService.ParseInvoiceFromXmlAsync(xmlStream, supplierId, createInvoiceDto.CustomerId);

        // Extract additional information
        xmlStream.Position = 0;
        invoice.DigitalSignature = await _xmlParserService.ExtractDigitalSignatureAsync(xmlStream);

        xmlStream.Position = 0;
        invoice.QRCode = await _xmlParserService.ExtractQRCodeAsync(xmlStream);

        // Upload file to S3
        fileStream.Position = 0;
        invoice.S3FileKey = await _fileStorageService.UploadFileAsync(
            fileStream,
            fileName,
            createInvoiceDto.InvoiceFile.ContentType);

        invoice.OriginalFileName = fileName;
        invoice.FileSizeBytes = createInvoiceDto.InvoiceFile.Length;

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        //TODO: Notify supplier and customer

        return await GetInvoiceByIdAsync(invoice.Id);
    }

    public async Task<InvoiceDto> GetInvoiceByIdAsync(Guid invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Supplier)
            .Include(i => i.Customer)
            .Include(i => i.InvoiceLines)
            .Include(i => i.InvoiceTaxes)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null)
        {
            throw new KeyNotFoundException("Invoice not found");
        }

        return _mapper.Map<InvoiceDto>(invoice);
    }

    public async Task<List<InvoiceDto>> GetInvoicesAsync(Guid? userId = null, int page = 1, int pageSize = 20)
    {
        var query = _context.Invoices
            .Include(i => i.Supplier)
            .Include(i => i.Customer)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(i => i.SupplierId == userId.Value || i.CustomerId == userId.Value);
        }

        var invoices = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<List<InvoiceDto>>(invoices);
    }

    public async Task<InvoiceDto> UpdateInvoiceStatusAsync(Guid invoiceId, InvoiceStatus status)
    {
        var invoice = await _context.Invoices.FindAsync(invoiceId);
        if (invoice == null)
        {
            throw new KeyNotFoundException("Invoice not found");
        }

        invoice.Status = status;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetInvoiceByIdAsync(invoiceId);
    }

    public async Task<string> GetInvoiceFileUrlAsync(Guid invoiceId)
    {
        var invoice = await _context.Invoices.FindAsync(invoiceId);
        if (invoice == null || string.IsNullOrEmpty(invoice.S3FileKey))
        {
            throw new KeyNotFoundException("Invoice file not found");
        }

        return await _fileStorageService.GetFileUrlAsync(invoice.S3FileKey, TimeSpan.FromHours(1));
    }

    public async Task<bool> DeleteInvoiceAsync(Guid invoiceId)
    {
        var invoice = await _context.Invoices.FindAsync(invoiceId);
        if (invoice == null) return false;

        // Soft delete
        invoice.IsDeleted = true;
        invoice.UpdatedAt = DateTime.UtcNow;

        // Optionally delete file from S3
        if (!string.IsNullOrEmpty(invoice.S3FileKey))
        {
            await _fileStorageService.DeleteFileAsync(invoice.S3FileKey);
        }

        await _context.SaveChangesAsync();
        return true;
    }
}