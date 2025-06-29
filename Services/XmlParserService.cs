using System.Xml.Linq;
using InvoiceProcessing.API.Services.Interfaces;

namespace InvoiceProcessing.API.Services;

public class XmlParserService : IXmlParserService
{
    public async Task<Invoice> ParseInvoiceFromXmlAsync(Stream xmlStream, Guid supplierId, Guid customerId)
    {
        xmlStream.Position = 0;
        var doc = await Task.Run(() => XDocument.Load(xmlStream));

        var ns = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
        var cbc = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
        var cac = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

        var invoice = new Invoice
        {
            SupplierId = supplierId,
            CustomerId = customerId,
            InvoiceNumber = doc.Root?.Element(cbc + "ID")?.Value ?? string.Empty,
            UUID = doc.Root?.Element(cbc + "UUID")?.Value ?? string.Empty,
            IssueDate = DateTime.Parse(doc.Root?.Element(cbc + "IssueDate")?.Value ?? DateTime.Now.ToString("yyyy-MM-dd")),
            IssueTime = TimeOnly.Parse(doc.Root?.Element(cbc + "IssueTime")?.Value ?? "00:00:00"),
            InvoiceTypeCode = doc.Root?.Element(cbc + "InvoiceTypeCode")?.Value ?? string.Empty,
            CurrencyCode = doc.Root?.Element(cbc + "DocumentCurrencyCode")?.Value ?? "SAR",
            LineCount = int.Parse(doc.Root?.Element(cbc + "LineCountNumeric")?.Value ?? "0"),
            Status = InvoiceStatus.Submitted
        };

        // Parse monetary totals
        var monetaryTotal = doc.Root?.Element(cac + "LegalMonetaryTotal");
        if (monetaryTotal != null)
        {
            invoice.NetAmount = decimal.Parse(monetaryTotal.Element(cbc + "LineExtensionAmount")?.Value ?? "0");
            invoice.TaxAmount = decimal.Parse(monetaryTotal.Element(cbc + "TaxInclusiveAmount")?.Value ?? "0") -
                               decimal.Parse(monetaryTotal.Element(cbc + "TaxExclusiveAmount")?.Value ?? "0");
            invoice.TotalAmount = decimal.Parse(monetaryTotal.Element(cbc + "TaxInclusiveAmount")?.Value ?? "0");
            invoice.DiscountAmount = decimal.Parse(monetaryTotal.Element(cbc + "AllowanceTotalAmount")?.Value ?? "0");
        }

        // Parse ICV from additional document references
        var icvElement = doc.Root?.Elements(cac + "AdditionalDocumentReference")
            .FirstOrDefault(x => x.Element(cbc + "ID")?.Value == "ICV");
        if (icvElement != null)
        {
            invoice.ICV = int.Parse(icvElement.Element(cbc + "UUID")?.Value ?? "0");
        }

        // Parse invoice lines
        var invoiceLines = doc.Root?.Elements(cac + "InvoiceLine") ?? new List<XElement>();
        foreach (var line in invoiceLines)
        {
            var invoiceLine = new InvoiceLine
            {
                LineNumber = int.Parse(line.Element(cbc + "ID")?.Value ?? "0"),
                Quantity = decimal.Parse(line.Element(cbc + "InvoicedQuantity")?.Value ?? "0"),
                UnitCode = line.Element(cbc + "InvoicedQuantity")?.Attribute("unitCode")?.Value ?? string.Empty,
                LineTotal = decimal.Parse(line.Element(cbc + "LineExtensionAmount")?.Value ?? "0"),
                ItemName = line.Element(cac + "Item")?.Element(cbc + "Name")?.Value ?? string.Empty,
                ItemCode = line.Element(cac + "Item")?.Element(cac + "AdditionalItemIdentification")?.Element(cbc + "ID")?.Value,
                BuyerItemId = line.Element(cac + "Item")?.Element(cac + "BuyersItemIdentification")?.Element(cbc + "ID")?.Value,
                UnitPrice = decimal.Parse(line.Element(cac + "Price")?.Element(cbc + "PriceAmount")?.Value ?? "0")
            };

            // Parse tax information for the line
            var taxTotal = line.Element(cac + "TaxTotal");
            if (taxTotal != null)
            {
                invoiceLine.TaxAmount = decimal.Parse(taxTotal.Element(cbc + "TaxAmount")?.Value ?? "0");
            }

            var taxCategory = line.Element(cac + "Item")?.Element(cac + "ClassifiedTaxCategory");
            if (taxCategory != null)
            {
                invoiceLine.TaxCategoryId = taxCategory.Element(cbc + "ID")?.Value ?? string.Empty;
                invoiceLine.TaxRate = decimal.Parse(taxCategory.Element(cbc + "Percent")?.Value ?? "0");
            }

            invoice.InvoiceLines.Add(invoiceLine);
        }

        // Parse tax totals
        var taxTotals = doc.Root?.Elements(cac + "TaxTotal") ?? new List<XElement>();
        foreach (var taxTotal in taxTotals.Take(1)) // Usually first one contains breakdown
        {
            var taxSubtotals = taxTotal.Elements(cac + "TaxSubtotal");
            foreach (var subtotal in taxSubtotals)
            {
                var invoiceTax = new InvoiceTax
                {
                    TaxableAmount = decimal.Parse(subtotal.Element(cbc + "TaxableAmount")?.Value ?? "0"),
                    TaxAmount = decimal.Parse(subtotal.Element(cbc + "TaxAmount")?.Value ?? "0"),
                    TaxCategoryId = subtotal.Element(cac + "TaxCategory")?.Element(cbc + "ID")?.Value ?? string.Empty,
                    TaxRate = decimal.Parse(subtotal.Element(cac + "TaxCategory")?.Element(cbc + "Percent")?.Value ?? "0"),
                    TaxSchemeId = subtotal.Element(cac + "TaxCategory")?.Element(cac + "TaxScheme")?.Element(cbc + "ID")?.Value ?? "VAT"
                };

                invoice.InvoiceTaxes.Add(invoiceTax);
            }
        }

        return invoice;
    }

    public async Task<bool> ValidateXmlStructureAsync(Stream xmlStream)
    {
        try
        {
            xmlStream.Position = 0;
            var doc = await Task.Run(() => XDocument.Load(xmlStream));

            // Basic validation - check if it's UBL Invoice
            var ns = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
            var root = doc.Root;

            return root?.Name.NamespaceName == ns.NamespaceName && root.Name.LocalName == "Invoice";
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> ExtractDigitalSignatureAsync(Stream xmlStream)
    {
        xmlStream.Position = 0;
        var doc = await Task.Run(() => XDocument.Load(xmlStream));

        var ds = XNamespace.Get("http://www.w3.org/2000/09/xmldsig#");
        var signatureElement = doc.Descendants(ds + "SignatureValue").FirstOrDefault();

        return signatureElement?.Value ?? string.Empty;
    }

    public async Task<string> ExtractQRCodeAsync(Stream xmlStream)
    {
        xmlStream.Position = 0;
        var doc = await Task.Run(() => XDocument.Load(xmlStream));

        var cbc = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
        var cac = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

        var qrElement = doc.Root?.Elements(cac + "AdditionalDocumentReference")
            .FirstOrDefault(x => x.Element(cbc + "ID")?.Value == "QR");

        return qrElement?.Element(cac + "Attachment")?.Element(cbc + "EmbeddedDocumentBinaryObject")?.Value ?? string.Empty;
    }
}