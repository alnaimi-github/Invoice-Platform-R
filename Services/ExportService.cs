using System.Text.Json;
using System.Xml.Linq;
using AutoMapper;
using InvoiceProcessing.API.DTOs.Invoices;
using InvoiceProcessing.API.Services.Interfaces;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace InvoiceProcessing.API.Services;

public class ExportService : IExportService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ExportService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<ExportResultDto> ExportInvoicesAsync(ExportRequestDto request, Guid? userId = null)
    {
        var query = _context.Invoices
            .Include(i => i.Supplier)
            .Include(i => i.Customer)
            .Include(i => i.InvoiceLines)
            .Include(i => i.InvoiceTaxes)
            .AsQueryable();

        // Apply filters
        if (userId.HasValue)
        {
            query = query.Where(i => i.SupplierId == userId.Value || i.CustomerId == userId.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(i => i.IssueDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(i => i.IssueDate <= request.EndDate.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(i => i.Status == request.Status.Value);
        }

        if (request.SupplierId.HasValue)
        {
            query = query.Where(i => i.SupplierId == request.SupplierId.Value);
        }

        if (request.CustomerId.HasValue)
        {
            query = query.Where(i => i.CustomerId == request.CustomerId.Value);
        }

        if (request.InvoiceIds?.Any() == true)
        {
            query = query.Where(i => request.InvoiceIds.Contains(i.Id));
        }

        var invoices = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
        var invoiceIds = invoices.Select(i => i.Id).ToList();

        byte[] data = request.Format switch
        {
            ExportFormat.Xml => await GenerateInvoiceXmlAsync(invoiceIds.First()), // Single invoice for XML
            ExportFormat.Excel => await GenerateInvoiceExcelAsync(invoiceIds),
            ExportFormat.Csv => await GenerateInvoiceCsvAsync(invoiceIds),
            ExportFormat.Pdf => await GenerateInvoicePdfAsync(invoiceIds.First()), // Single invoice for PDF
            ExportFormat.Json => await GenerateInvoiceJsonAsync(invoiceIds),
            _ => throw new ArgumentException("Unsupported export format")
        };

        var result = new ExportResultDto(
            FileName: GenerateFileName(request.Format, invoices.Count),
            ContentType: GetContentType(request.Format),
            Data: data,
            RecordCount: invoices.Count
        );

        return result;
    }

    public async Task<ExportResultDto> ExportSingleInvoiceAsync(Guid invoiceId, ExportFormat format)
    {
        byte[] data = format switch
        {
            ExportFormat.Xml => await GenerateInvoiceXmlAsync(invoiceId),
            ExportFormat.Excel => await GenerateInvoiceExcelAsync(new List<Guid> { invoiceId }),
            ExportFormat.Csv => await GenerateInvoiceCsvAsync(new List<Guid> { invoiceId }),
            ExportFormat.Pdf => await GenerateInvoicePdfAsync(invoiceId),
            ExportFormat.Json => await GenerateInvoiceJsonAsync(new List<Guid> { invoiceId }),
            _ => throw new ArgumentException("Unsupported export format")
        };

        return new ExportResultDto(
            FileName: GenerateFileName(format, 1, invoiceId.ToString()),
            ContentType: GetContentType(format),
            Data: data,
            RecordCount: 1
        );
    }

    public async Task<byte[]> GenerateInvoiceXmlAsync(Guid invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Supplier)
            .Include(i => i.Customer)
            .Include(i => i.InvoiceLines)
            .Include(i => i.InvoiceTaxes)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null)
            throw new KeyNotFoundException("Invoice not found");

        var xml = GenerateUblXml(invoice);
        return Encoding.UTF8.GetBytes(xml.ToString());
    }

    public async Task<byte[]> GenerateInvoiceExcelAsync(List<Guid> invoiceIds)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Supplier)
            .Include(i => i.Customer)
            .Include(i => i.InvoiceLines)
            .Include(i => i.InvoiceTaxes)
            .Where(i => invoiceIds.Contains(i.Id))
            .ToListAsync();

        using var package = new ExcelPackage();

        // Invoice Summary Sheet
        var summarySheet = package.Workbook.Worksheets.Add("Invoice Summary");
        CreateInvoiceSummarySheet(summarySheet, invoices);

        // Invoice Lines Sheet
        var linesSheet = package.Workbook.Worksheets.Add("Invoice Lines");
        CreateInvoiceLinesSheet(linesSheet, invoices);

        // Tax Details Sheet
        var taxSheet = package.Workbook.Worksheets.Add("Tax Details");
        CreateTaxDetailsSheet(taxSheet, invoices);

        return package.GetAsByteArray();
    }

    public async Task<byte[]> GenerateInvoiceCsvAsync(List<Guid> invoiceIds)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Supplier)
            .Include(i => i.Customer)
            .Include(i => i.InvoiceLines)
            .Include(i => i.InvoiceTaxes)
            .Where(i => invoiceIds.Contains(i.Id))
            .ToListAsync();

        var csv = new StringBuilder();

        // Headers
        csv.AppendLine("Invoice Number,UUID,Issue Date,Issue Time,Supplier Name,Supplier Tax ID,Customer Name,Customer Tax ID,Currency,Total Amount,Tax Amount,Net Amount,Status");

        // Data
        foreach (var invoice in invoices)
        {
            csv.AppendLine($"{invoice.InvoiceNumber},{invoice.UUID},{invoice.IssueDate:yyyy-MM-dd},{invoice.IssueTime}," +
                          $"{invoice.Supplier.CompanyName},{invoice.Supplier.TaxId},{invoice.Customer.CompanyName},{invoice.Customer.TaxId}," +
                          $"{invoice.CurrencyCode},{invoice.TotalAmount},{invoice.TaxAmount},{invoice.NetAmount},{invoice.Status}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(Guid invoiceId)
    {
        // This is a simplified PDF generation
        // In production, you would use a library like iTextSharp or QuestPDF
        var invoice = await _context.Invoices
            .Include(i => i.Supplier)
            .Include(i => i.Customer)
            .Include(i => i.InvoiceLines)
            .Include(i => i.InvoiceTaxes)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null)
            throw new KeyNotFoundException("Invoice not found");

        var html = GenerateInvoiceHtml(invoice);

        // Convert HTML to PDF (simplified - in production use proper PDF library)
        return Encoding.UTF8.GetBytes(html);
    }

    public async Task<byte[]> GenerateInvoiceJsonAsync(List<Guid> invoiceIds)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Supplier)
            .Include(i => i.Customer)
            .Include(i => i.InvoiceLines)
            .Include(i => i.InvoiceTaxes)
            .Where(i => invoiceIds.Contains(i.Id))
            .ToListAsync();

        var invoiceDtos = _mapper.Map<List<InvoiceDto>>(invoices);
        var json = JsonSerializer.Serialize(invoiceDtos, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return Encoding.UTF8.GetBytes(json);
    }

    private XDocument GenerateUblXml(Invoice invoice)
    {
        var ns = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
        var cbc = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
        var cac = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

        var doc = new XDocument(
            new XElement(ns + "Invoice",
                new XAttribute(XNamespace.Xmlns + "cbc", cbc),
                new XAttribute(XNamespace.Xmlns + "cac", cac),

                new XElement(cbc + "ID", invoice.InvoiceNumber),
                new XElement(cbc + "UUID", invoice.UUID),
                new XElement(cbc + "IssueDate", invoice.IssueDate.ToString("yyyy-MM-dd")),
                new XElement(cbc + "IssueTime", invoice.IssueTime.ToString("HH:mm:ss")),
                new XElement(cbc + "InvoiceTypeCode", invoice.InvoiceTypeCode),
                new XElement(cbc + "DocumentCurrencyCode", invoice.CurrencyCode),
                new XElement(cbc + "LineCountNumeric", invoice.LineCount),

                // Supplier Party
                new XElement(cac + "AccountingSupplierParty",
                    new XElement(cac + "Party",
                        new XElement(cac + "PartyTaxScheme",
                            new XElement(cbc + "CompanyID", invoice.Supplier.TaxId),
                            new XElement(cac + "TaxScheme",
                                new XElement(cbc + "ID", "VAT"))),
                        new XElement(cac + "PartyLegalEntity",
                            new XElement(cbc + "RegistrationName", invoice.Supplier.CompanyName)))),

                // Customer Party
                new XElement(cac + "AccountingCustomerParty",
                    new XElement(cac + "Party",
                        new XElement(cac + "PartyTaxScheme",
                            new XElement(cbc + "CompanyID", invoice.Customer.TaxId),
                            new XElement(cac + "TaxScheme",
                                new XElement(cbc + "ID", "VAT"))),
                        new XElement(cac + "PartyLegalEntity",
                            new XElement(cbc + "RegistrationName", invoice.Customer.CompanyName)))),

                // Tax Total
                new XElement(cac + "TaxTotal",
                    new XElement(cbc + "TaxAmount", new XAttribute("currencyID", invoice.CurrencyCode), invoice.TaxAmount),
                    invoice.InvoiceTaxes.Select(tax =>
                        new XElement(cac + "TaxSubtotal",
                            new XElement(cbc + "TaxableAmount", new XAttribute("currencyID", invoice.CurrencyCode), tax.TaxableAmount),
                            new XElement(cbc + "TaxAmount", new XAttribute("currencyID", invoice.CurrencyCode), tax.TaxAmount),
                            new XElement(cac + "TaxCategory",
                                new XElement(cbc + "ID", tax.TaxCategoryId),
                                new XElement(cbc + "Percent", tax.TaxRate),
                                new XElement(cac + "TaxScheme",
                                    new XElement(cbc + "ID", tax.TaxSchemeId)))))),

                // Legal Monetary Total
                new XElement(cac + "LegalMonetaryTotal",
                    new XElement(cbc + "LineExtensionAmount", new XAttribute("currencyID", invoice.CurrencyCode), invoice.NetAmount),
                    new XElement(cbc + "TaxExclusiveAmount", new XAttribute("currencyID", invoice.CurrencyCode), invoice.NetAmount),
                    new XElement(cbc + "TaxInclusiveAmount", new XAttribute("currencyID", invoice.CurrencyCode), invoice.TotalAmount),
                    new XElement(cbc + "PayableAmount", new XAttribute("currencyID", invoice.CurrencyCode), invoice.TotalAmount)),

                // Invoice Lines
                invoice.InvoiceLines.Select(line =>
                    new XElement(cac + "InvoiceLine",
                        new XElement(cbc + "ID", line.LineNumber),
                        new XElement(cbc + "InvoicedQuantity", new XAttribute("unitCode", line.UnitCode), line.Quantity),
                        new XElement(cbc + "LineExtensionAmount", new XAttribute("currencyID", invoice.CurrencyCode), line.LineTotal),
                        new XElement(cac + "Item",
                            new XElement(cbc + "Name", line.ItemName),
                            new XElement(cac + "ClassifiedTaxCategory",
                                new XElement(cbc + "ID", line.TaxCategoryId),
                                new XElement(cbc + "Percent", line.TaxRate),
                                new XElement(cac + "TaxScheme",
                                    new XElement(cbc + "ID", "VAT")))),
                        new XElement(cac + "Price",
                            new XElement(cbc + "PriceAmount", new XAttribute("currencyID", invoice.CurrencyCode), line.UnitPrice))))));

        return doc;
    }

    private void CreateInvoiceSummarySheet(ExcelWorksheet sheet, List<Invoice> invoices)
    {
        // Headers
        var headers = new[] { "Invoice Number", "UUID", "Issue Date", "Supplier", "Customer", "Total Amount", "Tax Amount", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            sheet.Cells[1, i + 1].Value = headers[i];
            sheet.Cells[1, i + 1].Style.Font.Bold = true;
            sheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        // Data
        for (int i = 0; i < invoices.Count; i++)
        {
            var invoice = invoices[i];
            var row = i + 2;

            sheet.Cells[row, 1].Value = invoice.InvoiceNumber;
            sheet.Cells[row, 2].Value = invoice.UUID;
            sheet.Cells[row, 3].Value = invoice.IssueDate.ToString("yyyy-MM-dd");
            sheet.Cells[row, 4].Value = invoice.Supplier.CompanyName;
            sheet.Cells[row, 5].Value = invoice.Customer.CompanyName;
            sheet.Cells[row, 6].Value = invoice.TotalAmount;
            sheet.Cells[row, 7].Value = invoice.TaxAmount;
            sheet.Cells[row, 8].Value = invoice.Status.ToString();
        }

        sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
    }

    private void CreateInvoiceLinesSheet(ExcelWorksheet sheet, List<Invoice> invoices)
    {
        var headers = new[] { "Invoice Number", "Line Number", "Item Name", "Quantity", "Unit Price", "Line Total", "Tax Amount" };
        for (int i = 0; i < headers.Length; i++)
        {
            sheet.Cells[1, i + 1].Value = headers[i];
            sheet.Cells[1, i + 1].Style.Font.Bold = true;
            sheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        int row = 2;
        foreach (var invoice in invoices)
        {
            foreach (var line in invoice.InvoiceLines)
            {
                sheet.Cells[row, 1].Value = invoice.InvoiceNumber;
                sheet.Cells[row, 2].Value = line.LineNumber;
                sheet.Cells[row, 3].Value = line.ItemName;
                sheet.Cells[row, 4].Value = line.Quantity;
                sheet.Cells[row, 5].Value = line.UnitPrice;
                sheet.Cells[row, 6].Value = line.LineTotal;
                sheet.Cells[row, 7].Value = line.TaxAmount;
                row++;
            }
        }

        sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
    }

    private void CreateTaxDetailsSheet(ExcelWorksheet sheet, List<Invoice> invoices)
    {
        var headers = new[] { "Invoice Number", "Tax Category", "Taxable Amount", "Tax Amount", "Tax Rate" };
        for (int i = 0; i < headers.Length; i++)
        {
            sheet.Cells[1, i + 1].Value = headers[i];
            sheet.Cells[1, i + 1].Style.Font.Bold = true;
            sheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        int row = 2;
        foreach (var invoice in invoices)
        {
            foreach (var tax in invoice.InvoiceTaxes)
            {
                sheet.Cells[row, 1].Value = invoice.InvoiceNumber;
                sheet.Cells[row, 2].Value = tax.TaxCategoryId;
                sheet.Cells[row, 3].Value = tax.TaxableAmount;
                sheet.Cells[row, 4].Value = tax.TaxAmount;
                sheet.Cells[row, 5].Value = tax.TaxRate;
                row++;
            }
        }

        sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
    }

    private string GenerateInvoiceHtml(Invoice invoice)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <title>Invoice {invoice.InvoiceNumber}</title>
            <style>
                body {{ font-family: Arial, sans-serif; margin: 20px; }}
                .header {{ text-align: center; margin-bottom: 30px; }}
                .invoice-details {{ margin-bottom: 20px; }}
                .parties {{ display: flex; justify-content: space-between; margin-bottom: 30px; }}
                .party {{ width: 45%; }}
                table {{ width: 100%; border-collapse: collapse; }}
                th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                th {{ background-color: #f2f2f2; }}
                .totals {{ margin-top: 20px; text-align: right; }}
            </style>
        </head>
        <body>
            <div class='header'>
                <h1>INVOICE</h1>
                <h2>{invoice.InvoiceNumber}</h2>
            </div>
            
            <div class='invoice-details'>
                <p><strong>Date:</strong> {invoice.IssueDate:yyyy-MM-dd}</p>
                <p><strong>Time:</strong> {invoice.IssueTime}</p>
                <p><strong>UUID:</strong> {invoice.UUID}</p>
            </div>
            
            <div class='parties'>
                <div class='party'>
                    <h3>Supplier</h3>
                    <p>{invoice.Supplier.CompanyName}</p>
                    <p>Tax ID: {invoice.Supplier.TaxId}</p>
                </div>
                <div class='party'>
                    <h3>Customer</h3>
                    <p>{invoice.Customer.CompanyName}</p>
                    <p>Tax ID: {invoice.Customer.TaxId}</p>
                </div>
            </div>
            
            <table>
                <thead>
                    <tr>
                        <th>Item</th>
                        <th>Quantity</th>
                        <th>Unit Price</th>
                        <th>Total</th>
                    </tr>
                </thead>
                <tbody>
                    {string.Join("", invoice.InvoiceLines.Select(line => $@"
                    <tr>
                        <td>{line.ItemName}</td>
                        <td>{line.Quantity} {line.UnitCode}</td>
                        <td>{line.UnitPrice:C}</td>
                        <td>{line.LineTotal:C}</td>
                    </tr>"))}
                </tbody>
            </table>
            
            <div class='totals'>
                <p><strong>Net Amount: {invoice.NetAmount:C}</strong></p>
                <p><strong>Tax Amount: {invoice.TaxAmount:C}</strong></p>
                <p><strong>Total Amount: {invoice.TotalAmount:C}</strong></p>
            </div>
        </body>
        </html>";
    }

    private string GenerateFileName(ExportFormat format, int recordCount, string? identifier = null)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var prefix = recordCount == 1 ? "Invoice" : "Invoices";
        var suffix = identifier != null ? $"_{identifier}" : "";

        return format switch
        {
            ExportFormat.Xml => $"{prefix}_Export{suffix}_{timestamp}.xml",
            ExportFormat.Excel => $"{prefix}_Export{suffix}_{timestamp}.xlsx",
            ExportFormat.Csv => $"{prefix}_Export{suffix}_{timestamp}.csv",
            ExportFormat.Pdf => $"{prefix}_Export{suffix}_{timestamp}.pdf",
            ExportFormat.Json => $"{prefix}_Export{suffix}_{timestamp}.json",
            _ => $"{prefix}_Export{suffix}_{timestamp}.txt"
        };
    }

    private string GetContentType(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.Xml => "application/xml",
            ExportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ExportFormat.Csv => "text/csv",
            ExportFormat.Pdf => "application/pdf",
            ExportFormat.Json => "application/json",
            _ => "application/octet-stream"
        };
    }
}